using System.Security.Cryptography;
using System.Text.Json;
using Npgsql;
using Xunit;

namespace StoreCMS.IntegrationTests;

[CollectionDefinition("PostgreSQL", DisableParallelization = true)]
public sealed class PostgreSqlCollection : ICollectionFixture<PostgreSqlRun> { }

public sealed class PostgreSqlRun : IAsyncLifetime
{
    internal readonly string RunId = Guid.NewGuid().ToString("N");
    private NpgsqlConnectionStringBuilder settings = null!;
    internal readonly List<OwnedDatabase> Databases = [];
    private readonly List<(string Name, uint Oid, bool CreateDb, string Password)> roles = [];
    private readonly Dictionary<string, ResourceLedger> ledger = new(StringComparer.Ordinal);
    private bool stopped, provisionerVerified;
    private string serverVersion = "NOT CONNECTED";
    private string? manifestPath;
    private ResourceLedger? active;
    private string phase = "NotStarted";
    private object? diagnostic;

    internal void Stop()
    {
        if (stopped) return;
        stopped = true;
        foreach (var entry in ledger.Values) entry.Preserve();
    }
    internal void Guard() { if (stopped) throw new InvalidOperationException("RunStoppedNoFurtherMutations"); }
    internal static string Quote(string name) { using var quote = new NpgsqlCommandBuilder(); return quote.QuoteIdentifier(name); }
    internal static async Task<NpgsqlConnection> Open(string value)
    {
        var connection = new NpgsqlConnection(value);
        try { await connection.OpenAsync(); return connection; }
        catch { await connection.DisposeAsync(); throw; }
    }
    internal static async Task<object?> Scalar(NpgsqlConnection connection, string sql, params (string Name, object Value)[] values)
    {
        await using var command = new NpgsqlCommand(sql, connection);
        foreach (var value in values)
            if (value.Value is uint oid) command.Parameters.AddWithValue(value.Name, NpgsqlTypes.NpgsqlDbType.Oid, oid);
            else command.Parameters.AddWithValue(value.Name, value.Value);
        var result = await command.ExecuteScalarAsync();
        return result is DBNull ? null : result;
    }
    internal static async Task Exec(NpgsqlConnection connection, string sql)
    {
        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync();
    }
    private void Phase(ResourceLedger entry, string name)
    {
        Guard();
        active = entry;
        phase = name;
        entry.Note(name);
        Save();
    }
    private void Diagnose(string code, string[]? components = null, long? sessions = null, Exception? error = null)
    {
        diagnostic = new
        {
            Utc = DateTime.UtcNow, Code = code, Phase = phase, Target = active?.Name,
            Kind = active?.Kind, Oid = active?.Oid, OwnerOid = active?.OwnerOid,
            ChangedComponents = components ?? [], SessionCount = sessions,
            ExceptionType = error?.GetType().Name, SqlState = (error as PostgresException)?.SqlState
        };
        active?.Note(phase, code);
    }
    private void Reject(string code, string[]? components = null, long? sessions = null)
    {
        Diagnose(code, components, sessions);
        Stop();
        Save();
        throw new FixtureSafetyException(code);
    }
    private async Task Ddl(NpgsqlConnection connection, string sql, ResourceLedger entry, bool drop = false)
    {
        Guard();
        active = entry;
        phase = drop ? "Drop" : "Create";
        if (drop) entry.BeginDrop(); else entry.BeginCreate();
        Save();
        try { await Exec(connection, sql); }
        catch (Exception error)
        {
            Diagnose("UnknownDdlOutcome", error: error);
            Stop();
            Save();
            throw SafeError(error, "UnknownDdlOutcome");
        }
        if (drop) entry.AcknowledgeDrop(); else entry.AcknowledgeCreate();
        Save();
    }
    private async Task MetadataDdl(NpgsqlConnection connection, string sql, ResourceLedger entry, string operation)
    {
        Phase(entry, operation + "Started");
        try { await Exec(connection, sql); }
        catch (Exception error)
        {
            Diagnose("UnknownDdlOutcome", error: error);
            Stop();
            Save();
            throw SafeError(error, "UnknownDdlOutcome");
        }
        entry.Note(operation + "Acknowledged");
        Save();
    }
    private async Task<NpgsqlConnection> Admin()
    {
        if (!provisionerVerified) throw new InvalidOperationException("ProvisionerNotVerified");
        var connection = await Open(settings.ConnectionString);
        try
        {
            // [Комментарий ИИ-агента]: Меняются только параметры собственной сессии, без изменения server config и чужих sessions.
            await Exec(connection, "SET log_statement='none'; SET log_duration=off; SET log_min_duration_statement=-1; SET log_min_duration_sample=-1; SET log_min_error_statement='panic'; SET log_parameter_max_length=0; SET log_parameter_max_length_on_error=0");
            return connection;
        }
        catch { await connection.DisposeAsync(); throw; }
    }
    public async Task InitializeAsync()
    {
        if (!PostgreSqlConfiguration.Enabled) return;
        try
        {
            phase = "Preflight";
            Save();
            settings = PostgreSqlConfiguration.Read();
            await using (var connection = await Open(settings.ConnectionString))
            {
                var valid = await Scalar(connection, """
                    SELECT current_database()='postgres' AND current_user=@user AND session_user=@user
                    AND inet_server_addr()=@host::inet AND inet_server_port()=@port
                    AND current_setting('server_version_num')::int/10000=18
                    AND (SELECT rolsuper AND rolcreatedb AND rolcreaterole FROM pg_roles WHERE rolname=current_user)
                    AND NOT EXISTS(SELECT 1 FROM pg_extension WHERE extname='pgaudit')
                    AND current_setting('shared_preload_libraries')=''
                    AND current_setting('log_parameter_max_length_on_error')='0'
                    """, ("user", settings.Username!), ("host", settings.Host!), ("port", settings.Port));
                if (!Equals(valid, true)) throw new FixtureSafetyException("IdentityMismatch");
                serverVersion = (string)(await Scalar(connection, "SHOW server_version"))!;
            }
            provisionerVerified = true;
            foreach (bool createDb in new[] { false, true })
            {
                await using var connection = await Admin();
                string name = "sc_t013_r_" + Guid.NewGuid().ToString("N");
                var entry = new ResourceLedger(RunId, "Role", name);
                ledger.Add(name, entry);
                Phase(entry, "RoleNameCheck");
                if (await Scalar(connection, "SELECT oid FROM pg_roles WHERE rolname=@name", ("name", name)) != null)
                    Reject("IdentityMismatch");
                string password = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
                await Ddl(connection, $"CREATE ROLE {Quote(name)} LOGIN NOSUPERUSER NOCREATEROLE NOREPLICATION NOBYPASSRLS {(createDb ? "CREATEDB" : "NOCREATEDB")} PASSWORD '{password}'", entry);
                Phase(entry, "RoleIdentityReadback");
                var oid = (uint)(await Scalar(connection, "SELECT oid FROM pg_roles WHERE rolname=@name", ("name", name)))!;
                entry.RecordOid(oid);
                roles.Add((name, oid, createDb, password));
                Phase(entry, "RoleMarker");
                await MetadataDdl(connection, $"COMMENT ON ROLE {Quote(name)} IS 'StoreCMS:T013:{RunId}'", entry, "RoleMarker");
                entry.ConfirmCreated();
                Save();
            }
        }
        catch (Exception error)
        {
            if (diagnostic == null) Diagnose(error is FixtureSafetyException guard ? guard.Code : "PreflightFailed", error: error);
            Stop();
            Save();
            throw SafeError(error, "Preflight");
        }
    }
    internal static Exception SafeError(Exception error, string stage) => new InvalidOperationException(
        stage + ": " + error.GetType().Name + (error is FixtureSafetyException guard ? "/" + guard.Code : "")
        + (error is PostgresException pg ? "/" + pg.SqlState : "") + Environment.NewLine + error.StackTrace);
    internal async Task<OwnedDatabase> Fresh(bool create = true, bool createDb = false)
    {
        Guard();
        var role = roles.Single(value => value.CreateDb == createDb);
        var db = new OwnedDatabase(this, "sc_t013_d_" + Guid.NewGuid().ToString("N"), role.Name, role.Oid,
            new NpgsqlConnectionStringBuilder(settings.ConnectionString) { Username = role.Name, Password = role.Password });
        var entry = new ResourceLedger(RunId, "Database", db.Name, db.Owner, db.OwnerOid);
        ledger.Add(db.Name, entry);
        Databases.Add(db);
        try
        {
            await using var admin = await Admin();
            Phase(entry, "DatabaseNameCheck");
            if (await Scalar(admin, "SELECT oid FROM pg_database WHERE datname=@name", ("name", db.Name)) != null)
                Reject("IdentityMismatch");
            if (create)
            {
                await Ddl(admin, $"CREATE DATABASE {Quote(db.Name)} OWNER {Quote(db.Owner)} TEMPLATE template0 ENCODING 'UTF8'", entry);
                await Register(db, admin);
                Phase(entry, "DatabaseIsolation");
                await MetadataDdl(admin, $"REVOKE CONNECT ON DATABASE {Quote(db.Name)} FROM PUBLIC", ledger[db.Name], "DatabaseIsolation");
            }
            else if (!createDb) entry.ExpectAbsent();
            Save();
            return db;
        }
        catch (Exception error)
        {
            if (diagnostic == null) Diagnose(error is FixtureSafetyException guard ? guard.Code : "DatabaseProvisioningFailed", error: error);
            Stop();
            Save();
            throw SafeError(error, "DatabaseProvisioning");
        }
    }
    private async Task Register(OwnedDatabase db, NpgsqlConnection admin)
    {
        var entry = ledger[db.Name];
        Phase(entry, "DatabaseIdentityReadback");
        await using var command = new NpgsqlCommand("SELECT oid,datdba FROM pg_database WHERE datname=@name", admin);
        command.Parameters.AddWithValue("name", db.Name);
        await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync() || reader.GetFieldValue<uint>(1) != db.OwnerOid) Reject("IdentityMismatch");
        db.Oid = reader.GetFieldValue<uint>(0);
        entry.RecordOid(db.Oid.Value);
        entry.ConfirmCreated();
        Save();
    }
    internal void BeginInstallation(OwnedDatabase db)
    {
        var entry = ledger[db.Name];
        Phase(entry, "ProductionInstallation");
        if (entry.Creation == CreationState.Planned)
        {
            entry.BeginCreate();
            Save();
        }
    }
    internal void EndInstallation(OwnedDatabase db, bool canStart, string code)
    {
        var entry = ledger[db.Name];
        if (entry.Creation != CreationState.CreateStarted) return;
        if (!canStart || code != "InstallationCompleted") Reject("UnknownInstallerCreateOutcome");
        entry.AcknowledgeCreate();
        Save();
    }
    internal async Task RegisterCreated(OwnedDatabase db)
    {
        Guard();
        await using var admin = await Admin();
        await Register(db, admin);
        Phase(ledger[db.Name], "DatabaseIsolation");
        await MetadataDdl(admin, $"REVOKE CONNECT ON DATABASE {Quote(db.Name)} FROM PUBLIC", ledger[db.Name], "DatabaseIsolation");
        Save();
    }
    internal async Task Case(Func<Task> body)
    {
        Guard();
        try { await body(); }
        catch (Exception error)
        {
            if (diagnostic == null) Diagnose(error is FixtureSafetyException guard ? guard.Code : "CaseFailed", error: error);
            Stop();
            Save();
            if (error is Xunit.Sdk.XunitException) throw;
            throw SafeError(error, "CaseStopped");
        }
    }
    private void Save()
    {
        if (!PostgreSqlConfiguration.Enabled) return;
        if (manifestPath == null)
        {
            string directory = Environment.GetEnvironmentVariable("STORECMS_T013_EVIDENCE")
                ?? throw new InvalidOperationException("EvidenceDirectoryRequired");
            Directory.CreateDirectory(directory);
            string path = Path.GetFullPath(Path.Combine(directory, RunId + ".json"));
            // [Комментарий ИИ-агента]: Новый RunId и CreateNew исключают перезапись исторического manifest; загрузки прежних ledger нет.
            using (var reserve = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None)) reserve.Flush(true);
            manifestPath = path;
        }
        int Created(string kind) => ledger.Values.Count(e => e.Kind == kind && e.Creation is CreationState.CreateAcknowledged or CreationState.CreatedConfirmed);
        int Deleted(string kind) => ledger.Values.Count(e => e.Kind == kind && e.Deletion is DeletionState.DropAcknowledged or DeletionState.AbsentReadbackConfirmed);
        var bytes = JsonSerializer.SerializeToUtf8Bytes(new
        {
            RunId, ServerVersion = serverVersion, Stopped = stopped, Phase = phase, Diagnostic = diagnostic,
            CreatedDatabases = Created("Database"), DeletedDatabases = Deleted("Database"),
            PreservedDatabases = Created("Database") - Deleted("Database"),
            CreatedRoles = Created("Role"), DeletedRoles = Deleted("Role"), PreservedRoles = Created("Role") - Deleted("Role"),
            Databases = Databases.Select(db => new { db.Name, db.Owner, db.OwnerOid, db.Oid, db.Verified }),
            Roles = roles.Select(role => new { role.Name, role.Oid, role.CreateDb }),
            Ledger = ledger.Values
        }, new JsonSerializerOptions { WriteIndented = true });
        string temporary = manifestPath + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            using (var file = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096, FileOptions.WriteThrough))
            {
                file.Write(bytes);
                file.Flush(true);
            }
            File.Move(temporary, manifestPath, true);
        }
        finally { if (File.Exists(temporary)) File.Delete(temporary); }
    }
    public async Task DisposeAsync()
    {
        if (!PostgreSqlConfiguration.Enabled || settings == null) return;
        try
        {
            if (stopped || !provisionerVerified) return;
            if (Databases.Any(db => !db.Verified)) Reject("UnverifiedCaseNoCleanup");
            await using var admin = await Admin();
            foreach (var absent in Databases.Where(db => !db.Oid.HasValue))
            {
                var entry = ledger[absent.Name];
                Phase(entry, "ExpectedAbsentReadback");
                if (entry.Creation != CreationState.ExpectedAbsent
                    || await Scalar(admin, "SELECT oid FROM pg_database WHERE datname=@name", ("name", absent.Name)) != null)
                    Reject("IdentityMismatch");
            }
            foreach (var db in Databases.Where(db => db.Oid.HasValue))
            {
                var entry = ledger[db.Name];
                Phase(entry, "DatabaseIdentityCheck");
                if (entry.RunId != RunId || entry.Creation != CreationState.CreatedConfirmed
                    || entry.Oid != db.Oid || entry.ExpectedOwner != db.Owner || entry.OwnerOid != db.OwnerOid)
                    Reject("IdentityMismatch");
                if (!Equals(await Scalar(admin, "SELECT oid=@oid AND datdba=@owner FROM pg_database WHERE datname=@name",
                    ("oid", db.Oid!.Value), ("owner", db.OwnerOid), ("name", db.Name)), true))
                    Reject("IdentityMismatch");
                Phase(entry, "SnapshotComparison");
                var changed = await db.ChangedSnapshotComponents();
                if (changed.Length != 0) Reject("SnapshotMismatch", changed);
                Phase(entry, "TargetSessionsCheck");
                long sessions = Convert.ToInt64(await Scalar(admin, "SELECT count(*) FROM pg_stat_activity WHERE datname=@name", ("name", db.Name)));
                if (sessions != 0) Reject("TargetSessionsPresent", sessions: sessions);
                await Ddl(admin, "DROP DATABASE " + Quote(db.Name), entry, drop: true);
                Phase(entry, "DatabaseAbsentReadback");
                if (await Scalar(admin, "SELECT oid FROM pg_database WHERE datname=@name OR oid=@oid",
                    ("name", db.Name), ("oid", db.Oid.Value)) != null) Reject("DropReadbackMismatch");
                entry.ConfirmAbsent();
                Save();
            }
            foreach (var role in roles)
            {
                var entry = ledger[role.Name];
                Phase(entry, "RoleIdentityCheck");
                if (entry.RunId != RunId || entry.Creation != CreationState.CreatedConfirmed || entry.Oid != role.Oid)
                    Reject("IdentityMismatch");
                bool identity = Equals(await Scalar(admin, """
                    SELECT oid=@oid AND NOT rolsuper AND NOT rolcreaterole AND NOT rolreplication AND NOT rolbypassrls
                    AND rolcreatedb=@createdb AND rolcanlogin AND pg_catalog.shobj_description(oid,'pg_authid')=@marker
                    AND NOT EXISTS(SELECT 1 FROM pg_shdepend WHERE refclassid='pg_authid'::regclass AND refobjid=@oid AND deptype<>'p')
                    AND NOT EXISTS(SELECT 1 FROM pg_auth_members WHERE roleid=@oid OR member=@oid)
                    FROM pg_roles WHERE rolname=@name
                    """, ("oid", role.Oid), ("createdb", role.CreateDb), ("marker", "StoreCMS:T013:" + RunId), ("name", role.Name)), true);
                if (!identity) Reject("IdentityMismatch");
                await Ddl(admin, "DROP ROLE " + Quote(role.Name), entry, drop: true);
                Phase(entry, "RoleAbsentReadback");
                if (await Scalar(admin, "SELECT oid FROM pg_roles WHERE rolname=@name OR oid=@oid",
                    ("name", role.Name), ("oid", role.Oid)) != null) Reject("DropReadbackMismatch");
                entry.ConfirmAbsent();
                Save();
            }
        }
        catch (Exception error)
        {
            if (diagnostic == null) Diagnose(error is FixtureSafetyException guard ? guard.Code : "CleanupInspectionFailed", error: error);
            Stop();
            Save();
            throw SafeError(error, "CleanupStopped");
        }
        finally { Save(); }
    }
}

internal sealed class OwnedDatabase
{
    private readonly PostgreSqlRun run;
    internal string Name { get; }
    internal string Owner { get; }
    internal uint OwnerOid { get; }
    internal uint? Oid { get; set; }
    internal bool Verified { get; private set; }
    private SnapshotFingerprint? snapshot;
    private readonly byte[] snapshotKey = RandomNumberGenerator.GetBytes(32);
    private readonly NpgsqlConnectionStringBuilder settings;
    internal string Settings => settings.ConnectionString;
    internal OwnedDatabase(PostgreSqlRun run, string name, string owner, uint ownerOid, NpgsqlConnectionStringBuilder settings)
    { this.run = run; Name = name; Owner = owner; OwnerOid = ownerOid; this.settings = settings; settings.Database = name; }
    internal Task<NpgsqlConnection> Open() { run.Guard(); return PostgreSqlRun.Open(Settings); }
    internal async Task<object?> Scalar(string sql, params (string Name, object Value)[] values)
    { run.Guard(); await using var connection = await Open(); return await PostgreSqlRun.Scalar(connection, sql, values); }
    internal async Task Exec(string sql)
    { run.Guard(); await using var connection = await Open(); await PostgreSqlRun.Exec(connection, sql); }
    internal void BeginInstallation() => run.BeginInstallation(this);
    internal void EndInstallation(bool canStart, string code) => run.EndInstallation(this, canStart, code);
    internal async Task Verify()
    {
        if (Verified || snapshot != null) throw new InvalidOperationException("BaselineAlreadyCaptured");
        if (Oid.HasValue) snapshot = await SnapshotFingerprint.Capture(this, snapshotKey);
        Verified = true;
    }
    internal async Task<string[]> ChangedSnapshotComponents()
    {
        if (snapshot == null) return ["MissingBaseline"];
        return snapshot.ChangedComponents(await SnapshotFingerprint.Capture(this, snapshotKey));
    }
}

internal sealed class FixtureSafetyException(string code) : InvalidOperationException(code)
{
    internal string Code { get; } = code;
}
