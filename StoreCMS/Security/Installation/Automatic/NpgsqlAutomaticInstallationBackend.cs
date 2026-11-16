using System;
using System.Data;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;
using Treynessen.Security.Installation.Bootstrap;

namespace Treynessen.Security.Installation.Automatic;

internal sealed class NpgsqlAutomaticInstallationBackend : IAutomaticInstallationBackend
{
    internal const string CompletedReceipt = "StoreCMS:E17a:completed:v1";
    internal const string CreatedReceipt = "StoreCMS:E17a:created:v1";
    internal const string SchemaReceipt = "StoreCMS:E17a:schema:v1";
    private readonly NpgsqlConnectionStringBuilder settings;
    private readonly InstallationTarget target;
    private readonly NpgsqlConnection maintenance;
    private uint databaseOid;
    private bool locked;
    private bool existingEmpty;
    private const string EmptyDatabaseSql = """
        NOT EXISTS(SELECT 1 FROM pg_catalog.pg_class c JOIN pg_catalog.pg_namespace n ON n.oid=c.relnamespace WHERE n.nspname='public')
        AND NOT EXISTS(SELECT 1 FROM pg_catalog.pg_proc p JOIN pg_catalog.pg_namespace n ON n.oid=p.pronamespace WHERE n.nspname='public')
        AND NOT EXISTS(SELECT 1 FROM pg_catalog.pg_type t JOIN pg_catalog.pg_namespace n ON n.oid=t.typnamespace WHERE n.nspname='public')
        AND NOT EXISTS(SELECT 1 FROM pg_catalog.pg_largeobject_metadata)
        AND NOT EXISTS(SELECT 1 FROM pg_catalog.pg_namespace WHERE nspname NOT IN ('public','pg_catalog','information_schema') AND left(nspname,3)<>'pg_')
        AND NOT EXISTS(SELECT 1 FROM pg_catalog.pg_event_trigger)
        AND NOT EXISTS(SELECT 1 FROM pg_catalog.pg_extension WHERE extname<>'plpgsql')
        """;
    internal NpgsqlAutomaticInstallationBackend(string value)
    {
        settings = NpgsqlBootstrapBackend.CreateWriteSettings(value);
        if (!IPAddress.TryParse(settings.Host, out _) || string.IsNullOrWhiteSpace(settings.Username)
            || !Regex.IsMatch(settings.Database ?? "", "^[a-zA-Z_][a-zA-Z0-9_]{0,62}$")
            || settings.Database is "postgres" or "template0" or "template1")
            throw new InvalidOperationException("ExplicitTargetRequired");
        target = new(settings.Database, settings.Username);
        var maintenanceSettings = new NpgsqlConnectionStringBuilder(settings.ConnectionString) { Database = "postgres" };
        maintenance = new(maintenanceSettings.ConnectionString);
    }
    private NpgsqlCommand MaintenanceCommand(string sql) => new(sql, maintenance) { CommandTimeout = 5 };
    public async Task<bool> LockAsync(CancellationToken token)
    {
        await maintenance.OpenAsync(token);
        await PostgreSqlConnectionIdentity.VerifyAsync(maintenance, null, token);
        await using var command = MaintenanceCommand("SELECT pg_catalog.pg_try_advisory_lock(@key)");
        command.Parameters.AddWithValue("key", LockKey(target.Database));
        locked = (bool)(await command.ExecuteScalarAsync(token) ?? false);
        return locked;
    }
    internal static long LockKey(string database) => System.Buffers.Binary.BinaryPrimitives.ReadInt64BigEndian(
        SHA256.HashData(Encoding.UTF8.GetBytes("StoreCMS:E17a:" + database)));
    public async Task CheckLeaseAsync(CancellationToken token)
    {
        if (!locked) throw new InvalidOperationException();
        await PostgreSqlConnectionIdentity.VerifyAsync(maintenance, null, token);
    }
    public async Task<DatabaseStage> InspectDatabaseAsync(CancellationToken token)
    {
        await CheckLeaseAsync(token);
        await using var command = MaintenanceCommand("""
            SELECT d.oid,d.datdba=(SELECT oid FROM pg_catalog.pg_roles WHERE rolname=current_user),
              pg_catalog.shobj_description(d.oid,'pg_database')
            FROM pg_catalog.pg_database d WHERE d.datname=@name
            """);
        command.Parameters.AddWithValue("name", target.Database);
        string receipt;
        existingEmpty = false;
        await using (var reader = await command.ExecuteReaderAsync(token))
        {
            if (!await reader.ReadAsync(token)) return DatabaseStage.Missing;
            databaseOid = reader.GetFieldValue<uint>(0);
            if (!reader.GetBoolean(1)) throw new InvalidOperationException("DatabaseOwnerMismatch");
            receipt = reader.IsDBNull(2) ? null : reader.GetString(2);
        }
        if (receipt == null)
        {
            await using var connection = new NpgsqlConnection(settings.ConnectionString);
            await connection.OpenAsync(token);
            await using var transaction = await connection.BeginTransactionAsync(IsolationLevel.RepeatableRead, token);
            await VerifyTargetAsync(connection, transaction, token);
            await using var empty = new NpgsqlCommand("SELECT " + EmptyDatabaseSql, connection, transaction);
            existingEmpty = Equals(await empty.ExecuteScalarAsync(token), true);
            if (existingEmpty) return DatabaseStage.Empty;
        }
        return receipt == CreatedReceipt ? DatabaseStage.CreatedByInstaller
            : receipt == SchemaReceipt ? DatabaseStage.SchemaReady : DatabaseStage.Existing;
    }
    public async Task<bool> CanCreateAsync(CancellationToken token)
    {
        await using var command = MaintenanceCommand("SELECT rolcreatedb OR rolsuper FROM pg_catalog.pg_roles WHERE rolname=current_user");
        return (bool)(await command.ExecuteScalarAsync(token) ?? false);
    }
    internal static string CreateDatabaseSql(string name, string role)
    {
        using var quote = new NpgsqlCommandBuilder();
        return "CREATE DATABASE " + quote.QuoteIdentifier(name) + " OWNER " + quote.QuoteIdentifier(role) + " TEMPLATE template0 ENCODING 'UTF8'";
    }
    private string ReceiptSql(string receipt)
    {
        using var quote = new NpgsqlCommandBuilder();
        if (receipt != CreatedReceipt && receipt != SchemaReceipt) throw new InvalidOperationException();
        return "COMMENT ON DATABASE " + quote.QuoteIdentifier(target.Database) + " IS '" + receipt + "'";
    }
    public async Task CreateDatabaseAsync(CancellationToken token)
    {
        await CheckLeaseAsync(token);
        await using (var command = MaintenanceCommand(CreateDatabaseSql(target.Database, target.Role)))
            await command.ExecuteNonQueryAsync(token);
        // [Комментарий ИИ-агента]: После сбоя между CREATE DATABASE и receipt продолжение допускается лишь при повторной проверке пустоты, владельца и identity.
        if (await InspectDatabaseAsync(token) != DatabaseStage.Empty) throw new InvalidOperationException();
        existingEmpty = false;
        await using var receipt = MaintenanceCommand(ReceiptSql(CreatedReceipt));
        await receipt.ExecuteNonQueryAsync(token);
    }
    private async Task VerifyTargetAsync(NpgsqlConnection connection, NpgsqlTransaction transaction, CancellationToken token)
    {
        await PostgreSqlConnectionIdentity.VerifyAsync(connection, transaction, token);
        await using var command = new NpgsqlCommand("SELECT oid=@oid AND datdba=(SELECT oid FROM pg_catalog.pg_roles WHERE rolname=current_user) FROM pg_catalog.pg_database WHERE datname=current_database()", connection, transaction);
        command.Parameters.AddWithValue("oid", NpgsqlTypes.NpgsqlDbType.Oid, databaseOid);
        if (!Equals(await command.ExecuteScalarAsync(token), true)) throw new InvalidOperationException("DatabaseProvenanceMismatch");
    }
    public async Task CreateSchemaAsync(CancellationToken token)
    {
        await CheckLeaseAsync(token);
        await using var connection = new NpgsqlConnection(settings.ConnectionString);
        await connection.OpenAsync(token);
        await using var transaction = await connection.BeginTransactionAsync(IsolationLevel.ReadCommitted, token);
        await VerifyTargetAsync(connection, transaction, token);
        await using (var command = new NpgsqlCommand("SELECT pg_catalog.pg_try_advisory_xact_lock(@ns,@key)", connection, transaction))
        {
            command.Parameters.AddWithValue("ns", NpgsqlBootstrapTransaction.AdvisoryNamespace);
            command.Parameters.AddWithValue("key", NpgsqlBootstrapTransaction.AdvisoryKey);
            if (!Equals(await command.ExecuteScalarAsync(token), true)) throw new InvalidOperationException("TargetBusy");
        }
        // [Комментарий ИИ-агента]: Пустая БД владельца допускается без receipt; перед DDL повторно проверяются пустота и прежний receipt в той же транзакции.
        await using (var command = new NpgsqlCommand("SELECT " + EmptyDatabaseSql + " AND (SELECT pg_catalog.shobj_description(oid,'pg_database') IS NOT DISTINCT FROM @receipt FROM pg_catalog.pg_database WHERE datname=current_database())", connection, transaction))
        {
            command.Parameters.AddWithValue("receipt", NpgsqlTypes.NpgsqlDbType.Text, existingEmpty ? DBNull.Value : CreatedReceipt);
            if (!Equals(await command.ExecuteScalarAsync(token), true)) throw new InvalidOperationException("UnknownDatabaseObjects");
        }
        using var stream = typeof(NpgsqlAutomaticInstallationBackend).Assembly.GetManifestResourceStream("StoreCMS.Installation.E17Schema.sql");
        using var reader = new StreamReader(stream);
        await using (var schema = new NpgsqlCommand(await reader.ReadToEndAsync(token), connection, transaction) { CommandTimeout = 30 })
            await schema.ExecuteNonQueryAsync(token);
        var catalog = await PostgreSqlInstallationValidator.ReadCatalogAsync(connection, transaction, "public", token, true);
        if (InstallationEvaluator.CheckCatalog(catalog, true) != null) throw new InvalidOperationException("CreatedSchemaRejected");
        await using (var receipt = new NpgsqlCommand(ReceiptSql(SchemaReceipt), connection, transaction))
            await receipt.ExecuteNonQueryAsync(token);
        await CheckLeaseAsync(token);
        await transaction.CommitAsync(token);
    }
    public async Task<InstallationResult> ValidateAsync(CancellationToken token)
    {
        await CheckLeaseAsync(token);
        await using var connection = new NpgsqlConnection(settings.ConnectionString);
        await connection.OpenAsync(token);
        await using var transaction = await connection.BeginTransactionAsync(IsolationLevel.RepeatableRead, token);
        await VerifyTargetAsync(connection, transaction, token);
        var catalog = await PostgreSqlInstallationValidator.ReadCatalogAsync(connection, transaction, "public", token, true);
        var failure = InstallationEvaluator.CheckCatalog(catalog, true);
        if (failure != null) return failure;
        var result = PostgreSqlInstallationValidator.EvaluateData(
            await PostgreSqlInstallationValidator.ReadDataAsync(connection, transaction, token, true), true);
        await VerifyReceiptAsync(connection, transaction, result, token);
        return result;
    }
    public async Task<BootstrapResult> BootstrapAsync(string initialPassword, CancellationToken token)
    {
        await CheckLeaseAsync(token);
        return await new AtomicBootstrapCoordinator(new NpgsqlBootstrapBackend(settings.ConnectionString, target, true, databaseOid),
            target, () => DateTime.UtcNow).InstallAsync(InitialInstallationPermission.AllowInitialInstallation, token, initialPassword);
    }
    internal static async Task VerifyReceiptAsync(NpgsqlConnection connection, NpgsqlTransaction transaction,
        InstallationResult result, CancellationToken token)
    {
        if (result.State != InstallationState.NotInitialized && !result.IsInstalled) return;
        await using var command = new NpgsqlCommand("SELECT pg_catalog.shobj_description(oid,'pg_database') FROM pg_catalog.pg_database WHERE datname=current_database()", connection, transaction);
        string expected = result.IsInstalled ? CompletedReceipt : SchemaReceipt;
        if (!Equals(await command.ExecuteScalarAsync(token), expected)) throw new InvalidOperationException("InstallationReceiptMismatch");
    }
    public async ValueTask DisposeAsync() => await maintenance.DisposeAsync();
}
