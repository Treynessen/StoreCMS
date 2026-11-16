using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;

namespace Treynessen.Security.Installation;

public static class InstallationEvaluator
{
    public static InstallationResult CheckIdentity(InstallationIdentity identity, InstallationTarget target)
    {
        if (identity == null || target == null || target.Schema != "public" || string.IsNullOrWhiteSpace(target.Database) || string.IsNullOrWhiteSpace(target.Role)
            || identity.Database != target.Database || identity.Role != target.Role || identity.Schema != target.Schema)
            return InstallationResult.Failure(InstallationState.SchemaRejected, "TargetMismatch", "Целевая база, роль или схема не совпадает с контрактом.");
        if (!identity.ReadOnly)
            return InstallationResult.Failure(InstallationState.Unavailable, "ReadOnlyRequired", "Требуется read-only транзакция.");
        if (identity.ServerMajor != 18)
            return InstallationResult.Failure(InstallationState.SchemaRejected, "ServerVersion", "Контракт проверен только для PostgreSQL 18.");
        return null;
    }

    public static InstallationResult CheckCatalog(InstallationCatalog catalog, bool automatic = false)
    {
        if (catalog?.Facts == null)
            return InstallationResult.Failure(InstallationState.SchemaRejected, "CatalogMissing", "Снимок каталогов недоступен.");
        if (automatic) catalog = new InstallationCatalog((JsonObject)catalog.Facts.DeepClone());
        var expected = InstallationManifest.LoadFacts(automatic);
        if (automatic && catalog.Facts["environment"] is JsonArray environments)
        {
            foreach (string flag in new[] { "super", "createDb", "createRole", "bypassRls", "replication", "schemaOwner" })
            {
                foreach (var row in environments) row.AsObject().Remove(flag);
                foreach (var row in expected["environment"].AsArray()) row.AsObject().Remove(flag);
            }
        }
        var diagnostics = new List<InstallationDiagnostic>();
        foreach (var category in expected)
        {
            if (catalog.Facts[category.Key] is not JsonArray actual || category.Value is not JsonArray wanted
                || !EquivalentRows(actual, wanted))
            {
                string label = "";
                if (catalog.Facts[category.Key] is JsonArray observed && category.Value is JsonArray required)
                {
                    var missing = required.FirstOrDefault(row => !observed.Any(candidate => JsonNode.DeepEquals(candidate, row)));
                    if (missing is JsonObject known)
                        label = " Ожидаемый объект: " + (known["table"] ?? known["name"])?.ToString()
                            + (known["column"] == null ? "" : "." + known["column"]) + ".";
                }
                diagnostics.Add(new("CatalogMismatch", "Контракт E1.3: несовпадение раздела " + category.Key + "." + label));
            }
        }
        if (catalog.Facts.Any(p => !expected.ContainsKey(p.Key)))
            diagnostics.Add(new("UnknownCatalogSection", "Обнаружен неизвестный раздел каталогов."));
        return diagnostics.Count == 0 ? null : new(InstallationState.SchemaRejected, diagnostics.Take(16).ToArray());
    }

    private static bool EquivalentRows(JsonArray actual, JsonArray expected)
    {
        if (actual.Count > 2048 || actual.Count != expected.Count) return false;
        var remaining = actual.ToList();
        foreach (var row in expected)
        {
            int match = remaining.FindIndex(candidate => JsonNode.DeepEquals(candidate, row));
            if (match < 0) return false;
            remaining.RemoveAt(match);
        }
        return true;
    }

    public static bool IsSupportedPasswordHash(string hash)
    {
        if (hash == null || hash.Length != 84) return false;
        Span<byte> payload = stackalloc byte[61];
        if (!Convert.TryFromBase64String(hash, payload, out int length) || length != 61 || payload[0] != 1) return false;
        uint prf = BinaryPrimitives.ReadUInt32BigEndian(payload.Slice(1, 4));
        uint iterations = BinaryPrimitives.ReadUInt32BigEndian(payload.Slice(5, 4));
        uint saltLength = BinaryPrimitives.ReadUInt32BigEndian(payload.Slice(9, 4));
        // [Комментарий ИИ-агента]: Ограниченный контракт V3 поддерживает проверенные SHA256/SHA512 и 600000 итераций; KDF здесь не выполняется.
        return (prf == 1 || prf == 2) && iterations == 600000 && saltLength == 16
            && Convert.ToBase64String(payload) == hash;
    }

    public static InstallationResult EvaluateData(InstallationData data, DateTime utcNow)
    {
        InstallationResult Recover(string code) => InstallationResult.Failure(InstallationState.RecoveryRequired, code,
            "Состояние установки противоречиво или не поддерживается; автоматическое исправление запрещено.");
        if (data?.NonEmptyTables == null || data.Markers == null || data.Roles == null || data.Users == null
            || data.Overflow || utcNow.Kind != DateTimeKind.Utc || utcNow.Year < 2000 || utcNow.Year > 9998
            || data.Markers.Any(m => m == null) || data.Roles.Any(r => r == null) || data.Users.Any(u => u == null)) return Recover("IncompleteData");
        var tables = InstallationManifest.ApplicationTables;
        if (data.NonEmptyTables.Count != tables.Count || tables.Any(t => !data.NonEmptyTables.ContainsKey(t))) return Recover("IncompleteTableState");
        if (data.NonEmptyTables["SecurityInstallation"] != (data.Markers.Count != 0)
            || data.NonEmptyTables["UserTypes"] != (data.Roles.Count != 0)
            || data.NonEmptyTables["Users"] != (data.Users.Count != 0)) return Recover("InconsistentSnapshot");
        if (data.Markers.Count == 0)
            return data.NonEmptyTables.Values.Any(v => v) ? Recover("UnmarkedData")
                : new(InstallationState.NotInitialized, Array.Empty<InstallationDiagnostic>());
        if (data.Markers.Count != 1) return Recover("MarkerCardinality");
        var marker = data.Markers[0];
        if (marker.Id != 0 || (marker.Version != 2 && marker.Version != 3) || !marker.LegacyResetCompleted
            || marker.InitializedUtc.Kind != DateTimeKind.Utc || marker.InitializedUtc.Year < 2000
            || marker.InitializedUtc > utcNow.AddMinutes(5)) return Recover("UnsupportedMarker");
        if (data.Roles.Select(r => r.Id).Distinct().Count() != data.Roles.Count
            || data.Users.Select(u => u.Id).Distinct().Count() != data.Users.Count
            || data.Users.Select(u => u.Login).Distinct(StringComparer.Ordinal).Count() != data.Users.Count
            || data.Roles.Any(r => string.IsNullOrWhiteSpace(r.Name) || r.AccessLevel < 1 || r.AccessLevel > 5)) return Recover("InvalidRolesOrUsers");
        var roles = data.Roles.ToDictionary(r => r.Id);
        if (data.Users.Any(u => string.IsNullOrWhiteSpace(u.Login) || !roles.ContainsKey(u.RoleId) || u.IdleTime < 10 || u.IdleTime > 10080)) return Recover("InvalidUserBinding");
        if (marker.Version == 3 && (!marker.SystemUserId.HasValue || !marker.SystemUserTypeId.HasValue
            || !roles.TryGetValue(marker.SystemUserTypeId.Value, out var systemRole)
            || systemRole.Name != "Admin" || systemRole.AccessLevel != 5
            || !data.Users.Any(u => u.Id == marker.SystemUserId && u.RoleId == marker.SystemUserTypeId)))
            return Recover("SystemIdentityMissing");
        var admins = data.Users.Where(u => roles[u.RoleId].AccessLevel == 5).ToList();
        if (admins.Count == 0) return Recover("AdministratorMissing");
        if (admins.Any(u => !IsSupportedPasswordHash(u.PasswordHash))) return Recover("UnsupportedAdministratorHash");
        return new(admins.Any(u => !u.RequirePasswordChange) ? InstallationState.InstalledReady
            : InstallationState.InstalledPasswordChangeRequired, Array.Empty<InstallationDiagnostic>());
    }
}
