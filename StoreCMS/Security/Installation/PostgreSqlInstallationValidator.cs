using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;

namespace Treynessen.Security.Installation;

public sealed class PostgreSqlInstallationValidator
{
    private readonly string connectionString;
    private readonly InstallationTarget target;
    private readonly bool automatic;
    public PostgreSqlInstallationValidator(string connectionString, InstallationTarget target, bool automatic = false)
    {
        this.connectionString = connectionString;
        this.target = target;
        this.automatic = automatic;
    }

    public async Task<InstallationResult> ValidateAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (target == null || target.Schema != "public" || string.IsNullOrWhiteSpace(target.Database) || string.IsNullOrWhiteSpace(target.Role))
                return InstallationResult.Failure(InstallationState.SchemaRejected, "InvalidTarget", "Ожидаемая цель должна явно задавать базу, роль и public.");
            using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            deadline.CancelAfter(TimeSpan.FromSeconds(20));
            var settings = CreateReadOnlySettings(connectionString);
            await using var connection = new NpgsqlConnection(settings.ConnectionString);
            await connection.OpenAsync(deadline.Token);
            await using var transaction = await connection.BeginTransactionAsync(IsolationLevel.RepeatableRead, deadline.Token);
            var identity = await ReadIdentityAsync(connection, transaction, target.Schema, deadline.Token);
            var failure = InstallationEvaluator.CheckIdentity(identity, target);
            if (failure != null) return failure;
            var catalog = await ReadCatalogAsync(connection, transaction, target.Schema, deadline.Token, automatic);
            failure = InstallationEvaluator.CheckCatalog(catalog, automatic);
            if (failure != null) return failure;
            var data = await ReadDataAsync(connection, transaction, deadline.Token, automatic);
            var result = EvaluateData(data, automatic);
            if (automatic) await Automatic.NpgsqlAutomaticInstallationBackend.VerifyReceiptAsync(connection, transaction, result, deadline.Token);
            await transaction.RollbackAsync(deadline.Token);
            return result;
        }
        catch (PostgresException error)
        {
            // [Комментарий ИИ-агента]: Текст PostgreSQL и объекты исключения могут содержать секреты; наружу выходит только код из закрытого набора.
            bool denied = error.SqlState == "42501";
            return InstallationResult.Failure(denied ? InstallationState.SchemaRejected : InstallationState.Unavailable,
                denied ? "CatalogAccessDenied" : "DatabaseFailure", denied ? "Недостаточно прав для проверки контракта." : "PostgreSQL не завершил проверку; установка не подтверждена.");
        }
        catch (OperationCanceledException)
        {
            return InstallationResult.Failure(InstallationState.Unavailable, "CancelledOrTimeout", "Проверка отменена или превышен лимит времени.");
        }
        catch (Exception)
        {
            return InstallationResult.Failure(InstallationState.Unavailable, "ValidationUnavailable", "Проверка недоступна; состояние установки не подтверждено.");
        }
    }

    internal static InstallationResult EvaluateData(InstallationData data, bool automatic) =>
        automatic && data.Markers.Any(m => m.Version != 3)
            ? InstallationResult.Failure(InstallationState.RecoveryRequired, "MarkerUpgradeRequired", "Требуется отдельное обновление маркера установки.")
            : InstallationEvaluator.EvaluateData(data, DateTime.UtcNow);

    internal static NpgsqlConnectionStringBuilder CreateReadOnlySettings(string value)
    {
        var settings = new NpgsqlConnectionStringBuilder(value)
        {
            Pooling = false, Timeout = 5, CommandTimeout = 5, IncludeErrorDetail = false,
            SearchPath = "public,pg_catalog", ApplicationName = "StoreCMS.InstallationValidator",
            Options = "-c default_transaction_read_only=on -c statement_timeout=5000 -c lock_timeout=1000 -c idle_in_transaction_session_timeout=10000",
            Multiplexing = false
        };
        settings.LogParameters = false;
        return settings;
    }

    internal static async Task<InstallationIdentity> ReadIdentityAsync(NpgsqlConnection connection, NpgsqlTransaction transaction, string schema, CancellationToken token)
    {
        await Automatic.PostgreSqlConnectionIdentity.VerifyAsync(connection, transaction, token);
        await using var command = new NpgsqlCommand("""
            SELECT pg_catalog.current_database(),current_user,
            COALESCE(pg_catalog.current_schema(),''),
            pg_catalog.current_setting('transaction_read_only')='on',
            pg_catalog.current_setting('server_version_num')::int/10000
            """, connection, transaction) { CommandTimeout = 5 };
        command.Parameters.AddWithValue("schema", schema);
        await using var reader = await command.ExecuteReaderAsync(token);
        if (!await reader.ReadAsync(token)) throw new InvalidOperationException();
        return new(reader.GetString(0), reader.GetString(1), reader.GetString(2), reader.GetBoolean(3), reader.GetInt32(4));
    }

    internal static async Task<InstallationCatalog> ReadCatalogAsync(NpgsqlConnection connection, NpgsqlTransaction transaction, string schema, CancellationToken token, bool automatic = false)
    {
        var facts = new JsonObject();
        foreach (var query in CatalogQueries.All)
        {
            await using var command = new NpgsqlCommand("SELECT CASE WHEN octet_length(fact::text)<=32768 THEN fact ELSE NULL END FROM (" + query.Value + ") AS catalog_rows(fact) LIMIT 2049", connection, transaction) { CommandTimeout = 5 };
            command.Parameters.AddWithValue("schema", schema);
            var rows = new JsonArray();
            await using var reader = await command.ExecuteReaderAsync(token);
            while (await reader.ReadAsync(token))
            {
                if (rows.Count >= 2048) throw new InvalidOperationException();
                string json = reader.GetString(0);
                if (json.Length > 32768) throw new InvalidOperationException();
                rows.Add(JsonNode.Parse(json));
            }
            facts.Add(query.Key, rows);
        }
        if (automatic)
        {
            var history = new JsonArray();
            await using var command = new NpgsqlCommand("SELECT \"MigrationId\",\"ProductVersion\" FROM public.\"__EFMigrationsHistory\" ORDER BY \"MigrationId\" LIMIT 4", connection, transaction);
            await using var reader = await command.ExecuteReaderAsync(token);
            while (await reader.ReadAsync(token)) history.Add(new JsonObject { ["id"] = reader.GetString(0), ["version"] = reader.GetString(1) });
            facts.Add("migrationHistory", history);
        }
        return new(facts);
    }

    internal static async Task<InstallationData> ReadDataAsync(NpgsqlConnection connection, NpgsqlTransaction transaction, CancellationToken token, bool automatic = false)
    {
        var nonEmpty = new Dictionary<string, bool>(StringComparer.Ordinal);
        using var quoting = new NpgsqlCommandBuilder();
        foreach (string table in InstallationManifest.ApplicationTables)
        {
            string qualified = quoting.QuoteIdentifier("public") + "." + quoting.QuoteIdentifier(table);
            await using var command = new NpgsqlCommand("SELECT EXISTS(SELECT 1 FROM " + qualified + ")", connection, transaction) { CommandTimeout = 5 };
            nonEmpty[table] = (bool)(await command.ExecuteScalarAsync(token) ?? throw new InvalidOperationException());
        }
        var markers = new List<InstallationMarker>();
        await using (var command = new NpgsqlCommand("SELECT \"ID\",\"SchemaVersion\",\"InitializedUtc\",\"LegacyPasswordResetCompleted\"" + (automatic ? ",\"SystemUserId\",\"SystemUserTypeId\"" : "") + " FROM public.\"SecurityInstallation\" ORDER BY \"ID\" LIMIT 3", connection, transaction))
        await using (var reader = await command.ExecuteReaderAsync(token))
            while (await reader.ReadAsync(token)) markers.Add(new(reader.GetInt32(0), reader.GetInt32(1), reader.GetDateTime(2), reader.GetBoolean(3), automatic && !reader.IsDBNull(4) ? reader.GetInt32(4) : null, automatic && !reader.IsDBNull(5) ? reader.GetInt32(5) : null));
        var roles = new List<InstallationRole>();
        await using (var command = new NpgsqlCommand("SELECT \"ID\",CASE WHEN length(\"Name\")<=256 THEN \"Name\" ELSE '' END,\"AccessLevel\" FROM public.\"UserTypes\" ORDER BY \"ID\" LIMIT 257", connection, transaction))
        await using (var reader = await command.ExecuteReaderAsync(token))
            while (await reader.ReadAsync(token)) roles.Add(new(reader.GetInt32(0), reader.GetString(1), reader.GetInt32(2)));
        var users = new List<InstallationUser>();
        await using (var command = new NpgsqlCommand("SELECT \"ID\",CASE WHEN length(\"Login\")<=256 THEN \"Login\" ELSE '' END,\"UserTypeID\",substring(\"Password\" from 1 for 85),\"IdleTime\",\"RequirePasswordChange\" FROM public.\"Users\" ORDER BY \"ID\" LIMIT 4097", connection, transaction))
        await using (var reader = await command.ExecuteReaderAsync(token))
            while (await reader.ReadAsync(token)) users.Add(new(reader.GetInt32(0), reader.GetString(1), reader.GetInt32(2), reader.GetString(3), reader.GetInt64(4), reader.GetBoolean(5)));
        return new(nonEmpty, markers.AsReadOnly(), roles.AsReadOnly(), users.AsReadOnly(), roles.Count > 256 || users.Count > 4096 || markers.Count > 2);
    }
}
