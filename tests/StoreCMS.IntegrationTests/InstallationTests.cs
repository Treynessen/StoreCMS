using System.Security.Cryptography;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Treynessen.AdminPanelTypes;
using Treynessen.Database;
using Npgsql;
using Treynessen.Database.Context;
using Treynessen.Security;
using Treynessen.Security.Installation;
using Treynessen.Security.Installation.Automatic;
using Treynessen.Security.Installation.Bootstrap;
using Xunit;

namespace StoreCMS.IntegrationTests;

[Collection("PostgreSQL")]
[Trait("Category", "Integration")]
public sealed class InstallationTests(PostgreSqlRun run)
{
    private const string CompletedReceipt = "StoreCMS:E17a:completed:v1";
    private static readonly string InitialPassword = Convert.ToHexString(RandomNumberGenerator.GetBytes(16));
    private static async Task<AutomaticInstallationResult> Install(OwnedDatabase db, bool providePassword = true)
    {
        db.BeginInstallation();
        var result = await AutomaticPostgreSqlInstallation.RunAsync(
            db.Settings, InitialInstallationPermission.AllowInitialInstallation,
            initialPassword: providePassword ? InitialPassword : null);
        db.EndInstallation(result.CanStart, result.Code);
        if (result.Code.EndsWith("OutcomeUnconfirmed", StringComparison.Ordinal)
            || result.Code is "CommitNotAcknowledged" or "UnknownCommitOutcome")
            throw new InvalidOperationException("UnknownInstallationOutcomeNoCleanup");
        return result;
    }
    private static async Task<string> Stage(OwnedDatabase db)
    {
        await using var backend = new InstallationBackend(db.Settings);
        Assert.True(await backend.Lock());
        return await backend.Inspect();
    }
    private static async Task Ready(OwnedDatabase db)
    {
        var result = await Install(db);
        Assert.True(result.CanStart, result.Code + "/" + result.DiagnosticCode);
        Assert.Equal("InstallationCompleted", result.Code);
    }
    private static async Task NoSchema(OwnedDatabase db)
    {
        Assert.Equal(0L, await db.Scalar("SELECT count(*) FROM pg_class c JOIN pg_namespace n ON n.oid=c.relnamespace WHERE n.nspname='public'"));
        Assert.Null(await db.Scalar("SELECT pg_catalog.shobj_description(oid,'pg_database') FROM pg_database WHERE datname=current_database()"));
    }

    [PostgreSqlFact, Trait("Area", "Installation")]
    public Task EmptyDatabaseInstallsRealSchemaMigrationsAndBootstrap() => run.Case(async () =>
    {
        var db = await run.Fresh();
        Assert.Equal("Empty", await Stage(db));
        await Ready(db);
        Assert.Equal("Existing", await Stage(db));
        Assert.Equal(CompletedReceipt, await db.Scalar("SELECT pg_catalog.shobj_description(oid,'pg_database') FROM pg_database WHERE datname=current_database()"));
        await using var context = new CMSDatabase(new DbContextOptionsBuilder<CMSDatabase>().UseNpgsql(db.Settings).Options);
        var user = await context.Users.SingleAsync();
        var role = await context.UserTypes.SingleAsync();
        var marker = await context.SecurityInstallation.SingleAsync();
        Assert.Equal("admin", user.Login);
        Assert.True(user.RequirePasswordChange);
        Assert.True(SecurityFunctions.VerifyPassword(user, InitialPassword));
        Assert.True(InstallationEvaluator.IsSupportedPasswordHash(user.Password));
        Assert.Equal("Admin", role.Name);
        Assert.Equal(5, (int)role.AccessLevel);
        Assert.Equal(3, marker.SchemaVersion);
        Assert.Equal(user.ID, marker.SystemUserId);
        Assert.Equal(role.ID, marker.SystemUserTypeId);
        Assert.Equal(2, (await context.Database.GetAppliedMigrationsAsync()).Count());
        Assert.Empty(await context.Database.GetPendingMigrationsAsync());
        var validation = await new PostgreSqlInstallationValidator(db.Settings, new(db.Name, db.Owner), true).ValidateAsync();
        Assert.Equal(InstallationState.InstalledPasswordChangeRequired, validation.State);
        await context.DisposeAsync();
        await db.Verify();
    });

    [PostgreSqlFact, Trait("Area", "InitialPassword")]
    public Task EmptyDatabaseWithoutPasswordRemainsEmpty() => run.Case(async () =>
    {
        var db = await run.Fresh();
        var result = await Install(db, providePassword: false);
        Assert.False(result.CanStart);
        Assert.Equal("InitialAdminPasswordRequiredOrInvalid", result.Code);
        Assert.Null(result.DiagnosticCode);
        await NoSchema(db);
        await db.Verify();
    });

    [PostgreSqlFact, Trait("Area", "InitialPassword")]
    public Task FirstLoginRequiresDifferentPasswordAndRestartPreservesIt() => run.Case(async () =>
    {
        var db = await run.Fresh();
        await Ready(db);
        var nextPassword = Convert.ToHexString(RandomNumberGenerator.GetBytes(16));
        var options = new DbContextOptionsBuilder<CMSDatabase>().UseNpgsql(db.Settings).Options;
        await using (var context = new CMSDatabase(options))
        {
            var account = await context.Users.SingleAsync();
            var login = new LoginFormModel { Login = account.Login, Password = InitialPassword };
            var request = new DefaultHttpContext();
            Assert.True(SecurityFunctions.IsValidLoginFormData(context, login, request, out var user));
            Assert.True(user.RequirePasswordChange);
            Assert.False(SecurityFunctions.HasAccessTo(AdminPanelPages.MainPage, user, request));

            var model = new UserModel { CurrentPassword = InitialPassword, NewPassword = InitialPassword, ConfirmPassword = InitialPassword };
            DatabaseInteraction.ChangeInitialPassword(context, user, model, out int reuseStatus);
            Assert.Equal(422, reuseStatus);
            Assert.True(user.RequirePasswordChange);
            model.NewPassword = model.ConfirmPassword = "x";
            DatabaseInteraction.ChangeInitialPassword(context, user, model, out int policyStatus);
            Assert.Equal(422, policyStatus);

            model.NewPassword = model.ConfirmPassword = nextPassword;
            DatabaseInteraction.ChangeInitialPassword(context, user, model, out int changeStatus);
            Assert.Equal(200, changeStatus);
            Assert.False(user.RequirePasswordChange);
        }
        var restart = await Install(db, providePassword: false);
        Assert.True(restart.CanStart);
        Assert.Equal("InstallationPreserved", restart.Code);
        await using (var context = new CMSDatabase(options))
        {
            var user = await context.Users.SingleAsync();
            Assert.False(user.RequirePasswordChange);
            Assert.False(SecurityFunctions.VerifyPassword(user, InitialPassword));
            Assert.True(SecurityFunctions.VerifyPassword(user, nextPassword));
        }
        await db.Verify();
    });

    [PostgreSqlFact, Trait("Area", "Installation")]
    public Task InstalledRestartPreservesRenamedLoginIdsHashAndInitialData() => run.Case(async () =>
    {
        var db = await run.Fresh();
        await Ready(db);
        await db.Exec("UPDATE public.\"Users\" SET \"Login\"='t013_owner'");
        string before = (string)(await db.Scalar("SELECT row_to_json(t)::text FROM (SELECT u.*,s.\"SystemUserId\",s.\"SystemUserTypeId\",s.\"InitializedUtc\" FROM public.\"Users\" u CROSS JOIN public.\"SecurityInstallation\" s) t"))!;
        for (int i = 0; i < 2; i++)
        {
            var result = await Install(db);
            Assert.True(result.CanStart);
            Assert.Equal("InstallationPreserved", result.Code);
        }
        Assert.True(before == (string)(await db.Scalar("SELECT row_to_json(t)::text FROM (SELECT u.*,s.\"SystemUserId\",s.\"SystemUserTypeId\",s.\"InitializedUtc\" FROM public.\"Users\" u CROSS JOIN public.\"SecurityInstallation\" s) t"))!);
        Assert.Equal(1L, await db.Scalar("SELECT count(*) FROM public.\"Users\""));
        Assert.Equal(1L, await db.Scalar("SELECT count(*) FROM public.\"UserTypes\""));
        Assert.Equal(1L, await db.Scalar("SELECT count(*) FROM public.\"SecurityInstallation\""));
        await db.Verify();
    });

    [PostgreSqlFact, Trait("Area", "Installation")]
    public Task MissingDatabaseWithCreatedbInstallsThroughProduction() => run.Case(async () =>
    {
        var db = await run.Fresh(create: false, createDb: true);
        Assert.Equal("Missing", await Stage(db));
        await Ready(db);
        await run.RegisterCreated(db);
        Assert.Equal(1L, await db.Scalar("SELECT count(*) FROM public.\"Users\""));
        await db.Verify();
    });

    [PostgreSqlFact, Trait("Area", "Installation")]
    public Task MissingDatabaseWithoutCreatedbStaysAbsent() => run.Case(async () =>
    {
        var db = await run.Fresh(create: false);
        Assert.Equal("Missing", await Stage(db));
        var result = await Install(db);
        Assert.False(result.CanStart);
        Assert.Equal("CreatedbPermissionRequired", result.Code);
        Assert.Equal("Missing", await Stage(db));
        await db.Verify();
    });

    private Task Unknown(string sql, string check) => run.Case(async () =>
    {
        var db = await run.Fresh();
        await db.Exec(sql);
        Assert.Equal("Existing", await Stage(db));
        Assert.False((await Install(db)).CanStart);
        Assert.True(Equals(await db.Scalar(check), true));
        Assert.Null(await db.Scalar("SELECT to_regclass('public.\"__EFMigrationsHistory\"')::text"));
        await db.Verify();
    });
    [PostgreSqlFact, Trait("Area", "Classification")]
    public Task ForeignTableIsPreserved() => Unknown("CREATE TABLE public.keep(value int); INSERT INTO public.keep VALUES(42)", "SELECT value=42 FROM public.keep");
    [PostgreSqlFact, Trait("Area", "Classification")]
    public Task ForeignViewIsPreserved() => Unknown("CREATE VIEW public.keep AS SELECT 42 AS value", "SELECT value=42 FROM public.keep");
    [PostgreSqlFact, Trait("Area", "Classification")]
    public Task ForeignSequenceIsPreserved() => Unknown("CREATE SEQUENCE public.keep START 42", "SELECT last_value=42 FROM public.keep");
    [PostgreSqlFact, Trait("Area", "Classification")]
    public Task ForeignFunctionIsPreserved() => Unknown("CREATE FUNCTION public.keep() RETURNS int LANGUAGE sql AS 'SELECT 42'", "SELECT public.keep()=42");
    [PostgreSqlFact, Trait("Area", "Classification")]
    public Task ForeignTypeIsPreserved() => Unknown("CREATE TYPE public.keep AS ENUM ('sentinel')", "SELECT 'sentinel'::public.keep::text='sentinel'");
    [PostgreSqlFact, Trait("Area", "Classification")]
    public Task ForeignSchemaIsPreserved() => Unknown("CREATE SCHEMA keep", "SELECT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname='keep')");

    [PostgreSqlFact, Trait("Area", "Classification")]
    public Task PartialApplicationSchemaIsRejected() => Unknown("CREATE TABLE public.\"Users\"(\"ID\" int); INSERT INTO public.\"Users\" VALUES(42)", "SELECT \"ID\"=42 FROM public.\"Users\"");

    private Task BadReceipt(string receipt) => run.Case(async () =>
    {
        var db = await run.Fresh();
        await db.Exec($"COMMENT ON DATABASE {PostgreSqlRun.Quote(db.Name)} IS '{receipt}'");
        Assert.Equal("Existing", await Stage(db));
        Assert.False((await Install(db)).CanStart);
        Assert.Equal(receipt, await db.Scalar("SELECT pg_catalog.shobj_description(oid,'pg_database') FROM pg_database WHERE datname=current_database()"));
        Assert.Equal(0L, await db.Scalar("SELECT count(*) FROM pg_class c JOIN pg_namespace n ON n.oid=c.relnamespace WHERE n.nspname='public'"));
        await db.Verify();
    });
    [PostgreSqlFact, Trait("Area", "Receipt")]
    public Task UnknownReceiptFailsClosed() => BadReceipt("t013-unknown");
    [PostgreSqlFact, Trait("Area", "Receipt")]
    public Task DamagedReceiptFailsClosed() => BadReceipt("StoreCMS:E17a:completed:broken");

    private Task DamagedInstalled(string sql, string check, string? expected = "UnknownOrDamagedDatabase") => run.Case(async () =>
    {
        var db = await run.Fresh();
        await Ready(db);
        await db.Exec(sql.Replace("{DATABASE}", PostgreSqlRun.Quote(db.Name)));
        var result = await Install(db);
        Assert.False(result.CanStart);
        if (expected != null) Assert.Equal(expected, result.Code);
        Assert.True(Equals(await db.Scalar(check), true));
        await db.Verify();
    });
    [PostgreSqlFact, Trait("Area", "Receipt")]
    public Task InstalledUnknownReceiptIsNotRepaired() => DamagedInstalled("COMMENT ON DATABASE {DATABASE} IS 't013-unknown'", "SELECT pg_catalog.shobj_description(oid,'pg_database')='t013-unknown' FROM pg_database WHERE datname=current_database()", "ValidationUnavailable");
    [PostgreSqlFact, Trait("Area", "Integrity")]
    public Task UnsupportedSchemaVersionIsPreserved() => DamagedInstalled("UPDATE public.\"SecurityInstallation\" SET \"SchemaVersion\"=999", "SELECT \"SchemaVersion\"=999 FROM public.\"SecurityInstallation\"");
    [PostgreSqlFact, Trait("Area", "Integrity")]
    public Task OldSchemaVersionIsNotUpgraded() => DamagedInstalled("UPDATE public.\"SecurityInstallation\" SET \"SchemaVersion\"=2", "SELECT \"SchemaVersion\"=2 FROM public.\"SecurityInstallation\"");
    [PostgreSqlFact, Trait("Area", "Integrity")]
    public Task MissingMigrationHistoryIsNotRecreated() => DamagedInstalled("DROP TABLE public.\"__EFMigrationsHistory\"", "SELECT to_regclass('public.\"__EFMigrationsHistory\"') IS NULL", "ValidationUnavailable");
    [PostgreSqlFact, Trait("Area", "Integrity")]
    public Task MissingMarkerIsNotRecreated() => DamagedInstalled("DELETE FROM public.\"SecurityInstallation\"", "SELECT count(*)=0 FROM public.\"SecurityInstallation\"");
    [PostgreSqlFact, Trait("Area", "Integrity")]
    public Task MissingSystemUserIsNotRecreated() => DamagedInstalled("DELETE FROM public.\"Users\"", "SELECT count(*)=0 FROM public.\"Users\"");
    [PostgreSqlFact, Trait("Area", "Integrity")]
    public Task DamagedSystemIdsAreNotRepaired() => DamagedInstalled("UPDATE public.\"SecurityInstallation\" SET \"SystemUserId\"=999999", "SELECT \"SystemUserId\"=999999 FROM public.\"SecurityInstallation\"");

    [PostgreSqlFact, Trait("Area", "H01")]
    public Task ForeignLargeObjectBeforeInspectBlocksInstallationAndSurvives() => run.Case(async () =>
    {
        var db = await run.Fresh();
        uint oid = (uint)(await db.Scalar("SELECT lo_from_bytea(0,decode('5430313300ff','hex'))"))!;
        Assert.Equal("Existing", await Stage(db));
        Assert.False((await Install(db)).CanStart);
        await AssertLargeObject(db, oid);
        await NoSchema(db);
        await db.Verify();
    });
    private static async Task AssertLargeObject(OwnedDatabase db, uint oid)
    {
        Assert.Equal("5430313300ff", await db.Scalar("SELECT encode(lo_get(oid),'hex') FROM pg_largeobject_metadata WHERE oid=@oid AND lomowner=(SELECT oid FROM pg_roles WHERE rolname=current_user)", ("oid", oid)));
    }
    [PostgreSqlFact, Trait("Area", "H01")]
    public Task LargeObjectBetweenInspectAndDdlBlocksSchemaAndSurvives() => run.Case(async () =>
    {
        var db = await run.Fresh();
        await using (var backend = new InstallationBackend(db.Settings))
        {
            Assert.True(await backend.Lock());
            Assert.Equal("Empty", await backend.Inspect());
            uint oid = (uint)(await db.Scalar("SELECT lo_from_bytea(0,decode('5430313300ff','hex'))"))!;
            var error = await Assert.ThrowsAsync<InvalidOperationException>(() => backend.Schema());
            Assert.Equal("UnknownDatabaseObjects", error.Message);
            await AssertLargeObject(db, oid);
            await NoSchema(db);
        }
        Assert.False((await Install(db)).CanStart);
        await db.Verify();
    });
    [PostgreSqlFact, Trait("Area", "Fault")]
    public Task TableBetweenInspectAndDdlBlocksSchemaAndSurvives() => run.Case(async () =>
    {
        var db = await run.Fresh();
        await using (var backend = new InstallationBackend(db.Settings))
        {
            Assert.True(await backend.Lock());
            Assert.Equal("Empty", await backend.Inspect());
            await db.Exec("CREATE TABLE public.keep(value int); INSERT INTO public.keep VALUES(42)");
            var error = await Assert.ThrowsAsync<InvalidOperationException>(() => backend.Schema());
            Assert.Equal("UnknownDatabaseObjects", error.Message);
            Assert.Equal(42, await db.Scalar("SELECT value FROM public.keep"));
        }
        Assert.Null(await db.Scalar("SELECT to_regclass('public.\"__EFMigrationsHistory\"')::text"));
        await db.Verify();
    });

    [PostgreSqlFact, Trait("Area", "Fault")]
    public Task CancelledBeforeDdlCanBeRetriedAfterConfirmedRollback() => run.Case(async () =>
    {
        var db = await run.Fresh();
        await using (var backend = new InstallationBackend(db.Settings))
        {
            Assert.True(await backend.Lock());
            Assert.Equal("Empty", await backend.Inspect());
            using var cancelled = new CancellationTokenSource();
            cancelled.Cancel();
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => backend.Schema(cancelled.Token));
        }
        await NoSchema(db);
        await Ready(db);
        await db.Verify();
    });

    [PostgreSqlFact, Trait("Area", "Concurrency")]
    public Task TwoInitiatorsUseRealAdvisoryLockAndLaterPreserveInstallation() => run.Case(async () =>
    {
        var db = await run.Fresh();
        var held = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var first = Task.Run(async () =>
        {
            await using var backend = new InstallationBackend(db.Settings);
            Assert.True(await backend.Lock());
            Assert.Equal("Empty", await backend.Inspect());
            held.SetResult();
            await release.Task.WaitAsync(TimeSpan.FromSeconds(20));
            await backend.Schema();
            Assert.Equal(BootstrapState.InstallationCompleted, (await backend.Bootstrap(InitialPassword)).State);
        });
        try
        {
            await held.Task.WaitAsync(TimeSpan.FromSeconds(20));
            var second = await Install(db);
            Assert.False(second.CanStart);
            Assert.Equal("InstallationBusy", second.Code);
        }
        finally { release.TrySetResult(); await first; }
        Assert.Equal("InstallationPreserved", (await Install(db)).Code);
        Assert.Equal(1L, await db.Scalar("SELECT count(*) FROM public.\"Users\""));
        await db.Verify();
    });

    [PostgreSqlFact, Trait("Area", "Fault")]
    public Task RealBootstrapTransactionFailureRollsBackInsertedRoleThenRetries() => run.Case(async () =>
    {
        var db = await run.Fresh();
        await using (var backend = new InstallationBackend(db.Settings))
        {
            Assert.True(await backend.Lock());
            Assert.Equal("Empty", await backend.Inspect());
            await backend.Schema();
            await BootstrapTransactionProbe.FailAndRollback(db.Settings, db.Name, db.Owner);
            Assert.Equal(0L, await db.Scalar("SELECT count(*) FROM public.\"UserTypes\""));
            Assert.Equal(0L, await db.Scalar("SELECT count(*) FROM public.\"Users\""));
            Assert.Equal(0L, await db.Scalar("SELECT count(*) FROM public.\"SecurityInstallation\""));
            Assert.Equal("StoreCMS:E17a:schema:v1", await db.Scalar("SELECT pg_catalog.shobj_description(oid,'pg_database') FROM pg_database WHERE datname=current_database()"));
        }
        await Ready(db);
        await db.Verify();
    });

    [PostgreSqlFact, Trait("Area", "Installation")]
    public Task SchemaReceiptResumesBootstrapWithoutDuplicatingMigrations() => run.Case(async () =>
    {
        var db = await run.Fresh();
        await using (var backend = new InstallationBackend(db.Settings))
        {
            Assert.True(await backend.Lock());
            Assert.Equal("Empty", await backend.Inspect());
            await backend.Schema();
        }
        Assert.Equal("SchemaReady", await Stage(db));
        Assert.Equal(0L, await db.Scalar("SELECT count(*) FROM public.\"Users\""));
        await Ready(db);
        Assert.Equal(2L, await db.Scalar("SELECT count(*) FROM public.\"__EFMigrationsHistory\""));
        await db.Verify();
    });
}
