using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;

namespace Treynessen.Security.Installation.Bootstrap;

internal sealed class NpgsqlBootstrapBackend : IBootstrapBackend
{
    private readonly string connectionString;
    private readonly InstallationTarget target;
    private readonly bool automatic;
    private readonly uint? expectedDatabaseOid;
    internal NpgsqlBootstrapBackend(string connectionString, InstallationTarget target, bool automatic = true, uint? expectedDatabaseOid = null)
    { this.connectionString = connectionString; this.target = target; this.automatic = automatic; this.expectedDatabaseOid = expectedDatabaseOid; }
    public Task<InstallationResult> InspectAsync(CancellationToken token) =>
        new PostgreSqlInstallationValidator(connectionString, target, automatic).ValidateAsync(token);

    internal static NpgsqlConnectionStringBuilder CreateWriteSettings(string value)
    {
        var settings = PostgreSqlInstallationValidator.CreateReadOnlySettings(value);
        settings.ApplicationName = "StoreCMS.ExplicitInitialInstallation";
        settings.Options = "-c default_transaction_read_only=off -c statement_timeout=5000 -c lock_timeout=1000 -c idle_in_transaction_session_timeout=10000";
        return settings;
    }

    public async Task<IBootstrapTransaction> BeginAsync(CancellationToken token)
    {
        var connection = new NpgsqlConnection(CreateWriteSettings(connectionString).ConnectionString);
        try
        {
            await connection.OpenAsync(token);
            var transaction = await connection.BeginTransactionAsync(IsolationLevel.ReadCommitted, token);
            return new NpgsqlBootstrapTransaction(connection, transaction, automatic, expectedDatabaseOid);
        }
        catch
        {
            await connection.DisposeAsync();
            throw;
        }
    }
}

internal sealed class NpgsqlBootstrapTransaction : IBootstrapTransaction
{
    // [Комментарий ИИ-агента]: Постоянная пара SCMS/INIT едина для установщиков всех версий протокола и не зависит от хеша процесса.
    internal const int AdvisoryNamespace = 0x53434D53;
    internal const int AdvisoryKey = 0x494E4954;
    internal const string PermissionSql = "SELECT pg_catalog.has_function_privilege(current_user,'pg_catalog.pg_try_advisory_xact_lock(integer,integer)','EXECUTE')";
    internal const string RoleSql = "INSERT INTO public.\"UserTypes\" (\"Name\",\"AccessLevel\") VALUES (@name,@level) RETURNING \"ID\"";
    internal const string UserSql = "INSERT INTO public.\"Users\" (\"Login\",\"Password\",\"RequirePasswordChange\",\"Email\",\"IdleTime\",\"UserTypeID\") VALUES (@login,@hash,TRUE,NULL,@idle,@role) RETURNING \"ID\"";
    internal const string MarkerSql = "INSERT INTO public.\"SecurityInstallation\" (\"ID\",\"SchemaVersion\",\"InitializedUtc\",\"LegacyPasswordResetCompleted\",\"SystemUserId\",\"SystemUserTypeId\") VALUES (@id,@version,@utc,TRUE,@userId,@roleId)";
    private readonly NpgsqlConnection connection;
    private readonly NpgsqlTransaction transaction;
    private readonly bool automatic;
    private readonly uint? expectedDatabaseOid;
    internal NpgsqlBootstrapTransaction(NpgsqlConnection connection, NpgsqlTransaction transaction, bool automatic = true, uint? expectedDatabaseOid = null)
    { this.connection = connection; this.transaction = transaction; this.automatic = automatic; this.expectedDatabaseOid = expectedDatabaseOid; }
    private NpgsqlCommand Command(string sql) => new(sql, connection, transaction) { CommandTimeout = 5 };
    public async Task<InstallationIdentity> ReadIdentityAsync(CancellationToken token)
    {
        var identity = await PostgreSqlInstallationValidator.ReadIdentityAsync(connection, transaction, "public", token);
        if (expectedDatabaseOid.HasValue)
        {
            await using var command = Command("SELECT oid=@oid FROM pg_catalog.pg_database WHERE datname=current_database()");
            command.Parameters.AddWithValue("oid", NpgsqlTypes.NpgsqlDbType.Oid, expectedDatabaseOid.Value);
            if (!Equals(await command.ExecuteScalarAsync(token), true)) throw new InvalidOperationException("DatabaseOidMismatch");
        }
        return identity;
    }
    public async Task<bool> HasLockPermissionAsync(CancellationToken token)
    {
        await using var command = Command(PermissionSql);
        return (bool)(await command.ExecuteScalarAsync(token) ?? false);
    }
    public async Task<bool> TryAdvisoryLockAsync(CancellationToken token)
    {
        await using var command = Command("SELECT pg_catalog.pg_try_advisory_xact_lock(@namespace,@key)");
        command.Parameters.AddWithValue("namespace", AdvisoryNamespace);
        command.Parameters.AddWithValue("key", AdvisoryKey);
        return (bool)(await command.ExecuteScalarAsync(token) ?? false);
    }
    internal static string BuildLockSql(IReadOnlyList<string> tables)
    {
        if (tables == null || !tables.SequenceEqual(BootstrapPlan.TableOrder, StringComparer.Ordinal)) throw new InvalidOperationException();
        using var quoting = new NpgsqlCommandBuilder();
        return "LOCK TABLE " + string.Join(",", tables.Select(t => "public." + quoting.QuoteIdentifier(t))) + " IN SHARE ROW EXCLUSIVE MODE NOWAIT";
    }
    public async Task LockTablesAsync(IReadOnlyList<string> orderedTables, CancellationToken token)
    {
        await using var command = Command(BuildLockSql(orderedTables));
        await command.ExecuteNonQueryAsync(token);
    }
    public async Task<InstallationResult> ValidateAfterLocksAsync(CancellationToken token)
    {
        var catalog = await PostgreSqlInstallationValidator.ReadCatalogAsync(connection, transaction, "public", token, automatic);
        var result = InstallationEvaluator.CheckCatalog(catalog, automatic)
            ?? PostgreSqlInstallationValidator.EvaluateData(await ReadDataAsync(token), automatic);
        if (automatic) await Automatic.NpgsqlAutomaticInstallationBackend.VerifyReceiptAsync(connection, transaction, result, token);
        return result;
    }
    public async Task<int> InsertRoleAsync(BootstrapPlan plan, CancellationToken token)
    {
        await using var command = Command(RoleSql);
        command.Parameters.AddWithValue("name", BootstrapPlan.RoleName);
        command.Parameters.AddWithValue("level", BootstrapPlan.RoleLevel);
        return (int)(await command.ExecuteScalarAsync(token) ?? throw new InvalidOperationException());
    }
    public async Task<int> InsertUserAsync(BootstrapPlan plan, int roleId, CancellationToken token)
    {
        await using var command = Command(UserSql);
        command.Parameters.AddWithValue("login", BootstrapPlan.Login);
        command.Parameters.AddWithValue("hash", plan.PasswordHash);
        command.Parameters.AddWithValue("idle", BootstrapPlan.IdleTime);
        command.Parameters.AddWithValue("role", roleId);
        return (int)(await command.ExecuteScalarAsync(token) ?? throw new InvalidOperationException());
    }
    public async Task InsertMarkerAsync(BootstrapPlan plan, int userId, int roleId, CancellationToken token)
    {
        await using var command = Command(MarkerSql);
        command.Parameters.AddWithValue("id", BootstrapPlan.MarkerId);
        command.Parameters.AddWithValue("version", BootstrapPlan.ProtocolVersion);
        command.Parameters.AddWithValue("utc", plan.InitializedUtc);
        command.Parameters.AddWithValue("userId", userId);
        command.Parameters.AddWithValue("roleId", roleId);
        if (await command.ExecuteNonQueryAsync(token) != 1) throw new InvalidOperationException();
        if (automatic)
        {
            using var quoting = new NpgsqlCommandBuilder();
            await using var receipt = Command("COMMENT ON DATABASE " + quoting.QuoteIdentifier(connection.Database)
                + " IS '" + Automatic.NpgsqlAutomaticInstallationBackend.CompletedReceipt + "'");
            await receipt.ExecuteNonQueryAsync(token);
        }
    }
    public Task<InstallationData> ReadDataAsync(CancellationToken token) =>
        PostgreSqlInstallationValidator.ReadDataAsync(connection, transaction, token, automatic);
    public Task CommitAsync(CancellationToken token) => transaction.CommitAsync(token);
    public Task RollbackAsync(CancellationToken token) => transaction.RollbackAsync(token);
    public async ValueTask DisposeAsync()
    {
        try { await transaction.DisposeAsync(); }
        finally { await connection.DisposeAsync(); }
    }
}
