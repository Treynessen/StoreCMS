using System;
using System.Collections.Generic;
using System.Linq;

namespace Treynessen.Security.Installation.Bootstrap;

internal sealed class BootstrapPlan
{
    internal const string Login = "admin";
    internal const string RoleName = "Admin";
    internal const int RoleLevel = 5;
    internal const long IdleTime = 10;
    internal const int MarkerId = 0;
    internal const int ProtocolVersion = 3;
    internal string PasswordHash { get; }
    internal DateTime InitializedUtc { get; }
    internal static readonly IReadOnlyList<string> TableOrder = Array.AsReadOnly(
        InstallationManifest.ApplicationTables.OrderBy(t => t, StringComparer.Ordinal).ToArray());

    private BootstrapPlan(string hash, DateTime utc) { PasswordHash = hash; InitializedUtc = utc; }
    internal static BootstrapPlan Create(DateTime utc, string initialPassword)
    {
        if (utc.Kind != DateTimeKind.Utc || utc.Year < 2000 || utc.Year > 9998) throw new InvalidOperationException();
        string hash = SecurityFunctions.GetPasswordHash(initialPassword);
        if (!InstallationEvaluator.IsSupportedPasswordHash(hash)
            || !SecurityFunctions.VerifyPassword(new Database.Entities.User { Password = hash }, initialPassword)) throw new InvalidOperationException();
        return new(hash, new DateTime(utc.Ticks - utc.Ticks % 10, DateTimeKind.Utc));
    }

    internal bool Matches(InstallationData data, int roleId, int userId)
    {
        if (data == null || data.Overflow || data.Markers?.Count != 1 || data.Roles?.Count != 1 || data.Users?.Count != 1
            || data.NonEmptyTables?.Count != TableOrder.Count) return false;
        foreach (string table in TableOrder)
            if (!data.NonEmptyTables.TryGetValue(table, out bool nonEmpty)
                || nonEmpty != (table is "SecurityInstallation" or "UserTypes" or "Users")) return false;
        var role = data.Roles[0]; var user = data.Users[0]; var marker = data.Markers[0];
        return role != null && user != null && marker != null
            && role.Id == roleId && role.Name == RoleName && role.AccessLevel == RoleLevel
            && user.Id == userId && user.RoleId == roleId && user.Login == Login && user.IdleTime == IdleTime
            && user.RequirePasswordChange && string.Equals(user.PasswordHash, PasswordHash, StringComparison.Ordinal)
            && marker.SystemUserId == userId && marker.SystemUserTypeId == roleId
            && marker.Id == MarkerId && marker.Version == ProtocolVersion && marker.LegacyResetCompleted
            && marker.InitializedUtc.Kind == DateTimeKind.Utc && marker.InitializedUtc == InitializedUtc;
    }
    public override string ToString() => "BootstrapPlan { данные скрыты }";
}
