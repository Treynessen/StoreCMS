using Treynessen.Security.Installation;
using Treynessen.Security.Installation.Automatic;
using Treynessen.Security.Installation.Bootstrap;

namespace StoreCMS.UnitTests.TestDoubles;

internal sealed class InstallationBackend : IAutomaticInstallationBackend
{
    internal DatabaseStage Stage { get; set; } = DatabaseStage.Missing;
    internal InstallationState State { get; set; } = InstallationState.NotInitialized;
    internal BootstrapState BootstrapState { get; set; } = BootstrapState.InstallationCompleted;
    internal bool LockAvailable { get; set; } = true;
    internal bool CanCreate { get; set; } = true;
    internal string? FaultAt { get; set; }
    internal List<string> Calls { get; } = [];
    internal const string SensitiveSentinel = "SYNTHETIC_ERROR_MUST_NOT_ESCAPE";

    private void Record(string operation)
    {
        Calls.Add(operation);
        if (operation == FaultAt)
            throw new IOException(SensitiveSentinel);
    }

    public Task<bool> LockAsync(CancellationToken token)
    {
        Record("lock");
        return Task.FromResult(LockAvailable);
    }

    public Task<DatabaseStage> InspectDatabaseAsync(CancellationToken token)
    {
        Record("inspect");
        return Task.FromResult(Stage);
    }

    public Task<bool> CanCreateAsync(CancellationToken token)
    {
        Record("canCreate");
        return Task.FromResult(CanCreate);
    }

    public Task CreateDatabaseAsync(CancellationToken token)
    {
        Record("create");
        return Task.CompletedTask;
    }

    public Task CreateSchemaAsync(CancellationToken token)
    {
        Record("schema");
        return Task.CompletedTask;
    }

    public Task CheckLeaseAsync(CancellationToken token)
    {
        Record("lease");
        return Task.CompletedTask;
    }

    public Task<InstallationResult> ValidateAsync(CancellationToken token)
    {
        Record("validate");
        return Task.FromResult(new InstallationResult(State, []));
    }

    public Task<BootstrapResult> BootstrapAsync(string initialPassword, CancellationToken token)
    {
        Record("bootstrap");
        return Task.FromResult(new BootstrapResult(BootstrapState, "SyntheticBootstrapResult"));
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
