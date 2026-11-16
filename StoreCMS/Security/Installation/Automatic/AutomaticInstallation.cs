using System;
using System.Threading;
using System.Threading.Tasks;
using Treynessen.Database;
using Treynessen.Security.Installation.Bootstrap;

namespace Treynessen.Security.Installation.Automatic;

public sealed record AutomaticInstallationResult(bool CanStart, string Code, string DiagnosticCode = null);
internal enum DatabaseStage { Missing, CreatedByInstaller, SchemaReady, Existing, Empty }
internal interface IAutomaticInstallationBackend : IAsyncDisposable
{
    Task<bool> LockAsync(CancellationToken token);
    Task<DatabaseStage> InspectDatabaseAsync(CancellationToken token);
    Task<bool> CanCreateAsync(CancellationToken token);
    Task CreateDatabaseAsync(CancellationToken token);
    Task CreateSchemaAsync(CancellationToken token);
    Task<InstallationResult> ValidateAsync(CancellationToken token);
    Task<BootstrapResult> BootstrapAsync(string initialPassword, CancellationToken token);
    Task CheckLeaseAsync(CancellationToken token);
}

internal sealed class AutomaticInstallationCoordinator
{
    private readonly IAutomaticInstallationBackend backend;
    internal AutomaticInstallationCoordinator(IAutomaticInstallationBackend backend) => this.backend = backend;
    internal async Task<AutomaticInstallationResult> RunAsync(CancellationToken token = default, string initialPassword = null)
    {
        string operation = "Preflight";
        try
        {
            if (!await backend.LockAsync(token)) return new(false, "InstallationBusy");
            var stage = await backend.InspectDatabaseAsync(token);
            if ((stage is DatabaseStage.Missing or DatabaseStage.Empty or DatabaseStage.CreatedByInstaller)
                && !DatabaseInteraction.CorrectPassword(initialPassword))
                return new(false, "InitialAdminPasswordRequiredOrInvalid");
            if (stage == DatabaseStage.Missing)
            {
                if (!await backend.CanCreateAsync(token)) return new(false, "CreatedbPermissionRequired");
                operation = "CreateDatabaseOutcomeUnconfirmed";
                await backend.CreateDatabaseAsync(token);
                stage = DatabaseStage.CreatedByInstaller;
            }
            if (stage is DatabaseStage.CreatedByInstaller or DatabaseStage.Empty)
            {
                await backend.CheckLeaseAsync(token);
                operation = "SchemaOutcomeUnconfirmed";
                await backend.CreateSchemaAsync(token);
                stage = DatabaseStage.SchemaReady;
            }
            operation = "ValidationUnavailable";
            var validation = await backend.ValidateAsync(token);
            if (validation.IsInstalled) return new(true, "InstallationPreserved");
            if (stage != DatabaseStage.SchemaReady || validation.State != InstallationState.NotInitialized)
                return new(false, "UnknownOrDamagedDatabase", validation.Diagnostics.Count > 0 ? validation.Diagnostics[0].Code : null);
            if (!DatabaseInteraction.CorrectPassword(initialPassword))
                return new(false, "InitialAdminPasswordRequiredOrInvalid");
            await backend.CheckLeaseAsync(token);
            operation = "BootstrapOutcomeUnconfirmed";
            var bootstrap = await backend.BootstrapAsync(initialPassword, token);
            if (bootstrap.State is not (BootstrapState.InstallationCompleted or BootstrapState.AlreadyInstalled))
                return new(false, bootstrap.Code);
            return new(true, "InstallationCompleted");
        }
        catch (Exception error)
        {
            // [Комментарий ИИ-агента]: При неизвестном исходе нет повторения или удаления; наружу не попадает текст исключения с реквизитами.
            return new(false, operation, error.GetType().Name + (error is Npgsql.PostgresException pg ? "/" + pg.SqlState : ""));
        }
    }
}

public static class AutomaticPostgreSqlInstallation
{
    public static async Task<AutomaticInstallationResult> RunAsync(string connectionString,
        InitialInstallationPermission permission = InitialInstallationPermission.Denied, CancellationToken token = default,
        string initialPassword = null)
    {
        if (permission != InitialInstallationPermission.AllowInitialInstallation) return new(false, "ExplicitPermissionRequired");
        try
        {
            using var deadline = CancellationTokenSource.CreateLinkedTokenSource(token);
            deadline.CancelAfter(TimeSpan.FromMinutes(2));
            await using var backend = new NpgsqlAutomaticInstallationBackend(connectionString);
            return await new AutomaticInstallationCoordinator(backend).RunAsync(deadline.Token, initialPassword);
        }
        catch (Exception) { return new(false, "InstallationUnavailable"); }
    }
}
