using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Treynessen.Security.Installation;

[JsonConverter(typeof(JsonStringEnumConverter<InstallationState>))]
public enum InstallationState
{
    SchemaRejected, NotInitialized, InstalledPasswordChangeRequired, InstalledReady, RecoveryRequired, Unavailable
}

public sealed record InstallationDiagnostic(string Code, string Message);
public sealed record InstallationResult(InstallationState State, IReadOnlyList<InstallationDiagnostic> Diagnostics)
{
    public bool IsInstalled => State is InstallationState.InstalledReady or InstallationState.InstalledPasswordChangeRequired;
    public static InstallationResult Failure(InstallationState state, string code, string message) =>
        new(state, Array.AsReadOnly(new[] { new InstallationDiagnostic(code, message) }));
}

public sealed record InstallationTarget(string Database, string Role, string Schema = "public");
public sealed record InstallationIdentity(string Database, string Role, string Schema, bool ReadOnly, int ServerMajor);
public sealed record InstallationMarker(int Id, int Version, DateTime InitializedUtc, bool LegacyResetCompleted, int? SystemUserId = null, int? SystemUserTypeId = null);
public sealed record InstallationRole(int Id, string Name, int AccessLevel);
public sealed record InstallationUser(int Id, string Login, int RoleId, [property: JsonIgnore] string PasswordHash, long IdleTime, bool RequirePasswordChange)
{
    public override string ToString() => "InstallationUser { данные скрыты }";
}
public sealed record InstallationData(IReadOnlyDictionary<string, bool> NonEmptyTables,
    IReadOnlyList<InstallationMarker> Markers, IReadOnlyList<InstallationRole> Roles,
    IReadOnlyList<InstallationUser> Users, bool Overflow = false);

// [Комментарий ИИ-агента]: Снимки каталогов отделены от транспорта для негативных тестов без изменения PostgreSQL.
public sealed record InstallationCatalog(JsonObject Facts);
