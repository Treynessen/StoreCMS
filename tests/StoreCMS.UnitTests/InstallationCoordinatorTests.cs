using System.Text.Json;
using StoreCMS.UnitTests.TestDoubles;
using Treynessen.Security.Installation;
using Treynessen.Security.Installation.Automatic;
using Treynessen.Security.Installation.Bootstrap;
using Xunit;

namespace StoreCMS.UnitTests;

[Trait("Category", "Unit")]
public sealed class InstallationCoordinatorTests : NetworkIsolatedTest
{
    private static readonly string InitialPassword = Convert.ToHexString(System.Security.Cryptography.RandomNumberGenerator.GetBytes(16));

    [Theory]
    [InlineData("Missing", "lock,inspect,canCreate,create,lease,schema,validate,lease,bootstrap")]
    [InlineData("Empty", "lock,inspect,lease,schema,validate,lease,bootstrap")]
    [InlineData("CreatedByInstaller", "lock,inspect,lease,schema,validate,lease,bootstrap")]
    [InlineData("SchemaReady", "lock,inspect,validate,lease,bootstrap")]
    public async Task InitialInstallationExecutesStagesInOrder(string stage, string expected)
    {
        var backend = new InstallationBackend { Stage = Enum.Parse<DatabaseStage>(stage) };
        var result = await new AutomaticInstallationCoordinator(backend).RunAsync(initialPassword: InitialPassword);

        Assert.True(result.CanStart);
        Assert.Equal("InstallationCompleted", result.Code);
        Assert.Equal(expected.Split(','), backend.Calls);
    }

    [Theory]
    [InlineData("Missing", null, "lock,inspect")]
    [InlineData("Empty", "", "lock,inspect")]
    [InlineData("CreatedByInstaller", " ", "lock,inspect")]
    [InlineData("SchemaReady", "x", "lock,inspect,validate")]
    public async Task InvalidInitialPasswordStopsBeforeMutation(string stage, string? initialPassword, string expected)
    {
        var backend = new InstallationBackend { Stage = Enum.Parse<DatabaseStage>(stage) };
        var result = await new AutomaticInstallationCoordinator(backend).RunAsync(initialPassword: initialPassword);

        Assert.False(result.CanStart);
        Assert.Equal("InitialAdminPasswordRequiredOrInvalid", result.Code);
        Assert.Null(result.DiagnosticCode);
        Assert.Equal(expected.Split(','), backend.Calls);
    }

    [Theory]
    [InlineData(InstallationState.InstalledReady)]
    [InlineData(InstallationState.InstalledPasswordChangeRequired)]
    public async Task InstalledDatabaseIsPreservedWithoutBootstrap(InstallationState state)
    {
        var backend = new InstallationBackend { Stage = DatabaseStage.Existing, State = state };
        var coordinator = new AutomaticInstallationCoordinator(backend);

        for (int attempt = 0; attempt < 2; attempt++)
        {
            var result = await coordinator.RunAsync();
            Assert.True(result.CanStart);
            Assert.Equal("InstallationPreserved", result.Code);
        }

        Assert.Equal(new[] { "lock", "inspect", "validate", "lock", "inspect", "validate" }, backend.Calls);
    }

    [Theory]
    [InlineData("Existing", InstallationState.NotInitialized)]
    [InlineData("Existing", InstallationState.RecoveryRequired)]
    [InlineData("SchemaReady", InstallationState.RecoveryRequired)]
    [InlineData("SchemaReady", InstallationState.SchemaRejected)]
    [InlineData("SchemaReady", InstallationState.Unavailable)]
    public async Task UnknownOrDamagedStateStopsBeforeBootstrap(string stage, InstallationState state)
    {
        var backend = new InstallationBackend { Stage = Enum.Parse<DatabaseStage>(stage), State = state };
        var result = await new AutomaticInstallationCoordinator(backend).RunAsync(initialPassword: InitialPassword);

        Assert.False(result.CanStart);
        Assert.Equal("UnknownOrDamagedDatabase", result.Code);
        Assert.Equal(new[] { "lock", "inspect", "validate" }, backend.Calls);
    }

    [Fact]
    public async Task MissingWithoutCreatedbStopsBeforeCreation()
    {
        var backend = new InstallationBackend { CanCreate = false };
        var result = await new AutomaticInstallationCoordinator(backend).RunAsync(initialPassword: InitialPassword);

        Assert.False(result.CanStart);
        Assert.Equal("CreatedbPermissionRequired", result.Code);
        Assert.Equal(new[] { "lock", "inspect", "canCreate" }, backend.Calls);
    }

    [Fact]
    public async Task BusyInstallationDoesNotInspectOrMutate()
    {
        var backend = new InstallationBackend { LockAvailable = false };
        var result = await new AutomaticInstallationCoordinator(backend).RunAsync(initialPassword: InitialPassword);

        Assert.False(result.CanStart);
        Assert.Equal("InstallationBusy", result.Code);
        Assert.Equal(new[] { "lock" }, backend.Calls);
    }

    [Theory]
    [InlineData("lock", "Preflight", "lock")]
    [InlineData("inspect", "Preflight", "lock,inspect")]
    [InlineData("canCreate", "Preflight", "lock,inspect,canCreate")]
    [InlineData("create", "CreateDatabaseOutcomeUnconfirmed", "lock,inspect,canCreate,create")]
    [InlineData("lease", "CreateDatabaseOutcomeUnconfirmed", "lock,inspect,canCreate,create,lease")]
    [InlineData("schema", "SchemaOutcomeUnconfirmed", "lock,inspect,canCreate,create,lease,schema")]
    [InlineData("validate", "ValidationUnavailable", "lock,inspect,canCreate,create,lease,schema,validate")]
    [InlineData("bootstrap", "BootstrapOutcomeUnconfirmed", "lock,inspect,canCreate,create,lease,schema,validate,lease,bootstrap")]
    public async Task BackendFailureStopsAtFailedOperationWithoutRetry(string fault, string code, string expected)
    {
        var backend = new InstallationBackend { FaultAt = fault };
        var result = await new AutomaticInstallationCoordinator(backend).RunAsync(initialPassword: InitialPassword);

        Assert.False(result.CanStart);
        Assert.Equal(code, result.Code);
        Assert.Equal("IOException", result.DiagnosticCode);
        Assert.DoesNotContain(InstallationBackend.SensitiveSentinel, JsonSerializer.Serialize(result));
        Assert.Equal(expected.Split(','), backend.Calls);
    }

    [Theory]
    [InlineData(BootstrapState.UnknownCommitOutcome)]
    [InlineData(BootstrapState.RecoveryRequired)]
    [InlineData(BootstrapState.Unavailable)]
    [InlineData(BootstrapState.SchemaRejected)]
    [InlineData(BootstrapState.InstallingBusy)]
    [InlineData(BootstrapState.AuthorizationRequired)]
    public async Task UnconfirmedBootstrapReturnsFailureWithoutRetryOrCleanup(BootstrapState state)
    {
        var backend = new InstallationBackend { Stage = DatabaseStage.SchemaReady, BootstrapState = state };
        var result = await new AutomaticInstallationCoordinator(backend).RunAsync(initialPassword: InitialPassword);

        Assert.False(result.CanStart);
        Assert.Equal("SyntheticBootstrapResult", result.Code);
        Assert.Equal(new[] { "lock", "inspect", "validate", "lease", "bootstrap" }, backend.Calls);
    }

    [Fact]
    public async Task BootstrapAlreadyInstalledIsAcceptedWithoutSecondBootstrap()
    {
        var backend = new InstallationBackend { Stage = DatabaseStage.SchemaReady, BootstrapState = BootstrapState.AlreadyInstalled };
        var result = await new AutomaticInstallationCoordinator(backend).RunAsync(initialPassword: InitialPassword);

        Assert.True(result.CanStart);
        Assert.Equal("InstallationCompleted", result.Code);
        Assert.Equal(1, backend.Calls.Count(call => call == "bootstrap"));
    }

    [Fact]
    public async Task PublicApiRequiresExplicitPermissionBeforeParsingConnection()
    {
        var result = await AutomaticPostgreSqlInstallation.RunAsync("not-a-connection");
        Assert.False(result.CanStart);
        Assert.Equal("ExplicitPermissionRequired", result.Code);
    }
}
