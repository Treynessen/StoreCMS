using System.Text.Json.Nodes;

namespace StoreCMS.IntegrationTests;

public static class FixtureOfflineChecks
{
    public static int Run()
    {
        int checks = 0;
        void Check(bool value, string code)
        {
            if (!value) throw new InvalidOperationException("OfflineCheckFailed:" + code);
            checks++;
        }
        void Rejected(Action action, string code)
        {
            try { action(); }
            catch (InvalidOperationException) { checks++; return; }
            throw new InvalidOperationException("OfflineCheckFailed:" + code);
        }
        string[] volatileFields =
        [
            "relpages", "reltuples", "relallvisible", "relallfrozen", "relfrozenxid", "relminmxid",
            "relhasindex", "relhasrules", "relhastriggers", "relhassubclass"
        ];
        var before = new JsonObject
        {
            ["oid"] = 101, ["relowner"] = 102, ["relnamespace"] = 2200,
            ["relname"] = "synthetic", ["relkind"] = "r", ["relacl"] = "synthetic_acl",
            ["relfilenode"] = 103, ["new_unknown_structural_field"] = "synthetic_value"
        };
        foreach (string field in volatileFields) before[field] = 1;
        string baseline = SnapshotFingerprint.NormalizeCatalogRow("pg_class", before.ToJsonString());
        foreach (string field in volatileFields)
        {
            var changed = before.DeepClone().AsObject();
            changed[field] = 999;
            Check(baseline == SnapshotFingerprint.NormalizeCatalogRow("pg_class", changed.ToJsonString()), "VolatileField:" + field);
        }
        foreach (string field in new[] { "oid", "relowner", "relnamespace", "relname", "relkind", "relacl", "relfilenode", "new_unknown_structural_field" })
        {
            var changed = before.DeepClone().AsObject();
            changed[field] = "changed_synthetic";
            Check(baseline != SnapshotFingerprint.NormalizeCatalogRow("pg_class", changed.ToJsonString()), "StructuralField:" + field);
        }
        Check(SnapshotFingerprint.NormalizeCatalogRow("pg_attribute", """{"reltuples":1}""")
            != SnapshotFingerprint.NormalizeCatalogRow("pg_attribute", """{"reltuples":2}"""), "NoCrossCatalogExclusion");
        Check(SnapshotFingerprint.NormalizeCatalogRow("pg_class", """{"oid":1,"relowner":2}""")
            == SnapshotFingerprint.NormalizeCatalogRow("pg_class", """{"relowner":2,"oid":1}"""), "PropertyOrder");
        foreach (string catalog in new[] { "pg_index", "pg_trigger", "pg_rewrite", "pg_inherits", "pg_attrdef", "pg_sequence", "pg_default_acl", "pg_policy", "pg_largeobject_metadata" })
            Check(SnapshotFingerprint.Catalogs.Contains(catalog), "StructuralCatalog:" + catalog);
        byte[] key = Enumerable.Range(1, 32).Select(value => (byte)value).ToArray();
        Check(SnapshotFingerprint.Digest(key, ["ab", "c"]) != SnapshotFingerprint.Digest(key, ["a", "bc"]), "LengthFraming");
        Check(SnapshotFingerprint.Digest(key, ["a|b", "c"]) != SnapshotFingerprint.Digest(key, ["a", "b|c"]), "DelimiterFraming");
        Check(SnapshotFingerprint.Digest(key, ["synthetic"]) != SnapshotFingerprint.Digest(new byte[32], ["synthetic"]), "RunKey");
        foreach (string component in new[] { "DatabaseIdentityAndAcl", "Catalog.pg_class", "TableData", "SequenceState", "LargeObjectContents" })
        {
            var first = new SnapshotFingerprint();
            var second = new SnapshotFingerprint();
            first.Add(component, key, ["synthetic_before"]);
            second.Add(component, key, ["synthetic_after"]);
            Check(first.ChangedComponents(second).SequenceEqual([component]), "ChangedComponent:" + component);
            Check(first.ChangedComponents(first).Length == 0, "UnchangedComponent:" + component);
            Check(first.ChangedComponents(new SnapshotFingerprint()).SequenceEqual([component]), "MissingComponent:" + component);
        }
        foreach (string objectCatalog in new[] { "pg_index", "pg_trigger", "pg_rewrite", "pg_inherits" })
        {
            var withObject = new SnapshotFingerprint();
            var withoutObject = new SnapshotFingerprint();
            withObject.Add("Catalog.pg_class", key, [baseline]);
            withoutObject.Add("Catalog.pg_class", key, [baseline]);
            withObject.Add("Catalog." + objectCatalog, key, ["""{"synthetic_object_id":104}"""]);
            withoutObject.Add("Catalog." + objectCatalog, key, []);
            Check(withObject.ChangedComponents(withoutObject).SequenceEqual(["Catalog." + objectCatalog]),
                "ObjectRemovalDetected:" + objectCatalog);
        }
        var normal = new ResourceLedger("synthetic_run", "Database", "synthetic_db", "synthetic_role", 102);
        Rejected(normal.BeginDrop, "DropBeforeCreate");
        normal.BeginCreate();
        normal.AcknowledgeCreate();
        Rejected(normal.ConfirmCreated, "UnconfirmedIdentity");
        normal.RecordOid(101);
        normal.ConfirmCreated();
        normal.BeginDrop();
        normal.AcknowledgeDrop();
        Check(normal.Deletion == DeletionState.DropAcknowledged, "DropAckSeparateFromReadback");
        normal.ConfirmAbsent();
        normal.Preserve();
        Check(normal.Creation == CreationState.CreatedConfirmed && normal.Deletion == DeletionState.AbsentReadbackConfirmed && !normal.Preserved, "CompleteLedger");
        Rejected(normal.BeginDrop, "NoSecondDrop");
        var unknownCreate = new ResourceLedger("synthetic_run", "Role", "synthetic_role");
        unknownCreate.BeginCreate();
        unknownCreate.Preserve();
        Check(unknownCreate.Creation == CreationState.UnknownCreateOutcome && unknownCreate.Preserved, "UnknownCreate");
        Rejected(unknownCreate.BeginCreate, "NoUnknownCreateRetry");
        var unknownDrop = new ResourceLedger("synthetic_run", "Database", "synthetic_db");
        unknownDrop.BeginCreate();
        unknownDrop.AcknowledgeCreate();
        unknownDrop.RecordOid(101);
        unknownDrop.ConfirmCreated();
        unknownDrop.BeginDrop();
        unknownDrop.Preserve();
        Check(unknownDrop.Deletion == DeletionState.UnknownDropOutcome && unknownDrop.Preserved, "UnknownDrop");
        Rejected(unknownDrop.BeginDrop, "NoUnknownDropRetry");
        var acknowledged = new ResourceLedger("synthetic_run", "Database", "synthetic_db");
        acknowledged.BeginCreate();
        acknowledged.AcknowledgeCreate();
        acknowledged.Preserve();
        Check(acknowledged.Creation == CreationState.CreateAcknowledged && acknowledged.Preserved, "AckRemainsKnown");
        var absent = new ResourceLedger("synthetic_run", "Database", "synthetic_absent");
        absent.ExpectAbsent();
        Rejected(absent.BeginCreate, "NoImplicitCreateAfterNegativeCase");
        Check(normal.Events.Any(e => e.Phase == "DropStarted") && normal.Events.Any(e => e.Phase == "DropAcknowledged")
            && normal.Events.Any(e => e.Phase == "AbsentReadbackConfirmed"), "LedgerHistory");
        foreach (string code in new[] { "IdentityMismatch", "SnapshotMismatch", "TargetSessionsPresent" })
            Check(PostgreSqlRun.SafeError(new FixtureSafetyException(code), "Offline").Message.Contains(code), "SafeGuardCode:" + code);
        Check(!PostgreSqlRun.SafeError(new InvalidOperationException("synthetic-secret-sentinel"), "Offline").Message.Contains("synthetic-secret-sentinel"),
            "NoRawExceptionMessage");
        return checks;
    }
}
