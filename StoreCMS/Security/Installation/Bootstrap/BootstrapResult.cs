using System.Text.Json.Serialization;

namespace Treynessen.Security.Installation.Bootstrap;

public enum InitialInstallationPermission { Denied, AllowInitialInstallation }

[JsonConverter(typeof(JsonStringEnumConverter<BootstrapState>))]
public enum BootstrapState
{
    AuthorizationRequired, InstallationCompleted, AlreadyInstalled, InstallingBusy,
    RecoveryRequired, SchemaRejected, Unavailable, UnknownCommitOutcome
}

public sealed record BootstrapResult(BootstrapState State, string Code, bool? RollbackConfirmed = null,
    InstallationState? ObservedState = null, bool CleanupFailed = false);
