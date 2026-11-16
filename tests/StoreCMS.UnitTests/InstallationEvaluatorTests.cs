using System.Text.Json.Nodes;
using StoreCMS.UnitTests.TestDoubles;
using Treynessen.Security.Installation;
using Treynessen.Security.Installation.Bootstrap;
using Xunit;

namespace StoreCMS.UnitTests;

[Trait("Category", "Unit")]
public sealed class InstallationEvaluatorTests : NetworkIsolatedTest
{
    private static readonly DateTime Now = new(2026, 11, 16, 12, 0, 0, DateTimeKind.Utc);
    private static readonly Lazy<BootstrapPlan> Plan = new(() => BootstrapPlan.Create(Now,
        Convert.ToHexString(System.Security.Cryptography.RandomNumberGenerator.GetBytes(16))));

    private static InstallationData Installed(bool changeRequired = true)
    {
        return new(
            InstallationManifest.ApplicationTables.ToDictionary(t => t, t => t is "Users" or "UserTypes" or "SecurityInstallation"),
            [new(0, 3, Plan.Value.InitializedUtc, true, 42, 73)],
            [new(73, "Admin", 5)],
            [new(42, "owner-renamed", 73, Plan.Value.PasswordHash, 10, changeRequired)]);
    }

    [Fact]
    public void EmptyCompleteSnapshotIsNotInitialized()
    {
        var data = new InstallationData(InstallationManifest.ApplicationTables.ToDictionary(t => t, _ => false), [], [], []);
        var result = InstallationEvaluator.EvaluateData(data, Now);

        Assert.Equal(InstallationState.NotInitialized, result.State);
        Assert.False(result.IsInstalled);
        Assert.Empty(result.Diagnostics);
    }

    [Theory]
    [InlineData(true, InstallationState.InstalledPasswordChangeRequired)]
    [InlineData(false, InstallationState.InstalledReady)]
    public void ValidMarkerUsesActualSystemIdsAndAllowsRenamedLogin(bool required, InstallationState state)
    {
        var result = InstallationEvaluator.EvaluateData(Installed(required), Now);
        Assert.Equal(state, result.State);
        Assert.True(result.IsInstalled);
        Assert.Empty(result.Diagnostics);
    }

    [Fact]
    public void BootstrapPlanMatchesOnlyExactInitialSystemRecords()
    {
        var plan = Plan.Value;
        var data = Installed() with { Users = [new(42, "admin", 73, plan.PasswordHash, 10, true)] };

        Assert.True(plan.Matches(data, 73, 42));
        Assert.False(plan.Matches(data, 1, 1));
        Assert.False(plan.Matches(data with { Users = [data.Users[0] with { RequirePasswordChange = false }] }, 73, 42));
        Assert.False(plan.Matches(data with { Markers = [data.Markers[0] with { SystemUserId = 1 }] }, 73, 42));
    }

    [Theory]
    [InlineData("overflow", "IncompleteData")]
    [InlineData("missing-table", "IncompleteTableState")]
    [InlineData("inconsistent-table", "InconsistentSnapshot")]
    [InlineData("no-marker", "UnmarkedData")]
    [InlineData("duplicate-marker", "MarkerCardinality")]
    [InlineData("bad-version", "UnsupportedMarker")]
    [InlineData("bad-marker-id", "UnsupportedMarker")]
    [InlineData("reset-incomplete", "UnsupportedMarker")]
    [InlineData("future-marker", "UnsupportedMarker")]
    [InlineData("missing-system-user", "SystemIdentityMissing")]
    [InlineData("missing-system-type", "SystemIdentityMissing")]
    [InlineData("wrong-system-user", "SystemIdentityMissing")]
    [InlineData("wrong-system-type", "SystemIdentityMissing")]
    [InlineData("demoted-system-type", "SystemIdentityMissing")]
    [InlineData("unsupported-hash", "UnsupportedAdministratorHash")]
    [InlineData("unknown-user-role", "InvalidUserBinding")]
    [InlineData("invalid-idle-time", "InvalidUserBinding")]
    [InlineData("duplicate-user", "InvalidRolesOrUsers")]
    public void PartialOrContradictorySnapshotRequiresRecovery(string corruption, string diagnostic)
    {
        var data = Installed();
        var tables = data.NonEmptyTables.ToDictionary(p => p.Key, p => p.Value);
        data = corruption switch
        {
            "overflow" => data with { Overflow = true },
            "missing-table" => data with { NonEmptyTables = tables.Where(p => p.Key != "Chunks").ToDictionary() },
            "inconsistent-table" => data with { NonEmptyTables = tables.Select(p => new KeyValuePair<string, bool>(p.Key, p.Key == "Users" ? false : p.Value)).ToDictionary() },
            "no-marker" => data with { Markers = [], NonEmptyTables = tables.Select(p => new KeyValuePair<string, bool>(p.Key, p.Key == "SecurityInstallation" ? false : p.Value)).ToDictionary() },
            "duplicate-marker" => data with { Markers = [data.Markers[0], data.Markers[0]] },
            "bad-version" => data with { Markers = [data.Markers[0] with { Version = 99 }] },
            "bad-marker-id" => data with { Markers = [data.Markers[0] with { Id = 1 }] },
            "reset-incomplete" => data with { Markers = [data.Markers[0] with { LegacyResetCompleted = false }] },
            "future-marker" => data with { Markers = [data.Markers[0] with { InitializedUtc = Now.AddMinutes(6) }] },
            "missing-system-user" => data with { Markers = [data.Markers[0] with { SystemUserId = null }] },
            "missing-system-type" => data with { Markers = [data.Markers[0] with { SystemUserTypeId = null }] },
            "wrong-system-user" => data with { Markers = [data.Markers[0] with { SystemUserId = 1 }] },
            "wrong-system-type" => data with { Markers = [data.Markers[0] with { SystemUserTypeId = 1 }] },
            "demoted-system-type" => data with { Roles = [data.Roles[0] with { AccessLevel = 4 }] },
            "unsupported-hash" => data with { Users = [data.Users[0] with { PasswordHash = "not-an-identity-hash" }] },
            "unknown-user-role" => data with { Users = [data.Users[0] with { RoleId = 999 }] },
            "invalid-idle-time" => data with { Users = [data.Users[0] with { IdleTime = 9 }] },
            "duplicate-user" => data with { Users = [data.Users[0], data.Users[0]] },
            _ => throw new ArgumentOutOfRangeException(nameof(corruption))
        };

        var result = InstallationEvaluator.EvaluateData(data, Now);
        Assert.Equal(InstallationState.RecoveryRequired, result.State);
        Assert.False(result.IsInstalled);
        Assert.Equal(diagnostic, Assert.Single(result.Diagnostics).Code);
    }

    [Fact]
    public void MissingSystemUserIsNotReplacedByAnotherAdministrator()
    {
        var data = Installed();
        var result = InstallationEvaluator.EvaluateData(data with { Users = [data.Users[0] with { Id = 99 }] }, Now);
        Assert.Equal(InstallationState.RecoveryRequired, result.State);
        Assert.Equal("SystemIdentityMissing", Assert.Single(result.Diagnostics).Code);
    }

    [Fact]
    public void LocalClockIsRejectedInsteadOfGuessingUtc()
    {
        var result = InstallationEvaluator.EvaluateData(Installed(), DateTime.SpecifyKind(Now, DateTimeKind.Local));
        Assert.Equal("IncompleteData", Assert.Single(result.Diagnostics).Code);
    }

    [Fact]
    public void NullSnapshotIsRejected()
    {
        var result = InstallationEvaluator.EvaluateData(null!, Now);
        Assert.Equal(InstallationState.RecoveryRequired, result.State);
        Assert.Equal("IncompleteData", Assert.Single(result.Diagnostics).Code);
    }

    [Fact]
    public void AutomaticManifestIsAcceptedWithoutMutatingCaller()
    {
        var facts = InstallationManifest.LoadFacts(true);
        string before = facts.ToJsonString();

        Assert.Null(InstallationEvaluator.CheckCatalog(new(facts), true));
        Assert.Equal(before, facts.ToJsonString());
    }

    [Theory]
    [InlineData("foreign-key", "CatalogMismatch")]
    [InlineData("migration", "CatalogMismatch")]
    [InlineData("missing-section", "CatalogMismatch")]
    [InlineData("unknown-section", "UnknownCatalogSection")]
    public void ChangedOrUnknownCatalogIsRejected(string corruption, string diagnostic)
    {
        var facts = InstallationManifest.LoadFacts(true);
        switch (corruption)
        {
            case "foreign-key":
                facts["constraints"]!.AsArray().Single(r => (string?)r!["table"] == "Users" && (string?)r["kind"] == "f")!["delete"] = "c";
                break;
            case "migration":
                facts["migrationHistory"]!.AsArray()[0]!["id"] = "unknown_migration";
                break;
            case "missing-section":
                facts.Remove("constraints");
                break;
            case "unknown-section":
                facts["unknown"] = new JsonArray();
                break;
        }

        var result = InstallationEvaluator.CheckCatalog(new(facts), true);
        Assert.NotNull(result);
        Assert.Equal(InstallationState.SchemaRejected, result.State);
        Assert.Contains(result.Diagnostics, d => d.Code == diagnostic);
    }

    [Theory]
    [InlineData("database", "TargetMismatch")]
    [InlineData("role", "TargetMismatch")]
    [InlineData("schema", "TargetMismatch")]
    [InlineData("writable", "ReadOnlyRequired")]
    [InlineData("server-version", "ServerVersion")]
    public void IncorrectIdentityCannotBeAccepted(string mismatch, string diagnostic)
    {
        var identity = new InstallationIdentity("synthetic", "synthetic_role", "public", true, 18);
        identity = mismatch switch
        {
            "database" => identity with { Database = "other" },
            "role" => identity with { Role = "other" },
            "schema" => identity with { Schema = "other" },
            "writable" => identity with { ReadOnly = false },
            "server-version" => identity with { ServerMajor = 17 },
            _ => throw new ArgumentOutOfRangeException(nameof(mismatch))
        };
        var result = InstallationEvaluator.CheckIdentity(identity, new("synthetic", "synthetic_role"));

        Assert.NotNull(result);
        Assert.Equal(diagnostic, Assert.Single(result.Diagnostics).Code);
    }

    [Fact]
    public void MatchingReadOnlyPg18IdentityIsAccepted()
    {
        Assert.Null(InstallationEvaluator.CheckIdentity(new("synthetic", "synthetic_role", "public", true, 18), new("synthetic", "synthetic_role")));
    }
}
