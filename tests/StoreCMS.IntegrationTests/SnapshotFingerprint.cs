using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Npgsql;

namespace StoreCMS.IntegrationTests;

internal sealed class SnapshotFingerprint
{
    private readonly Dictionary<string, string> components = new(StringComparer.Ordinal);
    internal static readonly string[] Catalogs =
    [
        "pg_namespace", "pg_class", "pg_attribute", "pg_constraint", "pg_index", "pg_proc",
        "pg_type", "pg_trigger", "pg_extension", "pg_largeobject_metadata", "pg_description",
        "pg_attrdef", "pg_rewrite", "pg_inherits", "pg_sequence", "pg_default_acl", "pg_policy", "pg_depend"
    ];
    private static readonly string[] VolatileClassFields =
    [
        "relpages", "reltuples", "relallvisible", "relallfrozen", "relfrozenxid", "relminmxid",
        "relhasindex", "relhasrules", "relhastriggers", "relhassubclass"
    ];

    internal static string NormalizeCatalogRow(string catalog, string json)
    {
        var row = JsonNode.Parse(json)?.AsObject() ?? throw new InvalidOperationException("InvalidCatalogRow");
        // [Комментарий ИИ-агента]: Удаляем только служебные поля pg_class; фактические объекты проверяются соответствующими каталогами.
        if (catalog == "pg_class")
            foreach (string field in VolatileClassFields) row.Remove(field);
        return new JsonObject(row.OrderBy(pair => pair.Key, StringComparer.Ordinal)
            .Select(pair => KeyValuePair.Create(pair.Key, pair.Value?.DeepClone()))).ToJsonString();
    }
    internal static string Digest(byte[] key, IEnumerable<string> rows)
    {
        using var hash = IncrementalHash.CreateHMAC(HashAlgorithmName.SHA256, key);
        Span<byte> length = stackalloc byte[4];
        foreach (string row in rows)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(row);
            BinaryPrimitives.WriteInt32BigEndian(length, bytes.Length);
            hash.AppendData(length);
            hash.AppendData(bytes);
        }
        return Convert.ToHexString(hash.GetHashAndReset());
    }
    internal void Add(string name, byte[] key, IEnumerable<string> rows) => components.Add(name, Digest(key, rows));
    internal string[] ChangedComponents(SnapshotFingerprint other) => components.Keys.Union(other.components.Keys, StringComparer.Ordinal)
        .Where(name => !components.TryGetValue(name, out var before) || !other.components.TryGetValue(name, out var after) || before != after)
        .Order(StringComparer.Ordinal).ToArray();

    private static async Task<List<string>> Rows(NpgsqlConnection connection, NpgsqlTransaction transaction, string sql)
    {
        await using var command = new NpgsqlCommand(sql, connection, transaction);
        var rows = new List<string>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync()) rows.Add(reader.GetString(0));
        return rows;
    }
    internal static async Task<SnapshotFingerprint> Capture(OwnedDatabase db, byte[] key)
    {
        if (!db.Oid.HasValue) throw new FixtureSafetyException("IdentityMismatch");
        await using var connection = await db.Open();
        await using var transaction = await connection.BeginTransactionAsync(System.Data.IsolationLevel.RepeatableRead);
        await using (var readOnly = new NpgsqlCommand("SET TRANSACTION READ ONLY", connection, transaction))
            await readOnly.ExecuteNonQueryAsync();
        await using (var identity = new NpgsqlCommand("""
            SELECT jsonb_build_object('database',current_database(),'role',current_user,'sessionUser',session_user,
                'oid',d.oid,'owner',d.datdba,'acl',d.datacl,'receipt',pg_catalog.shobj_description(d.oid,'pg_database'))::text
            FROM pg_catalog.pg_database d WHERE d.datname=current_database() AND d.datname=@name
              AND d.oid=@oid AND d.datdba=@owner AND current_user=@role AND session_user=@role
              AND host(inet_server_addr())=@host AND inet_server_port()=@port
              AND current_setting('server_version_num')::int/10000=18
            """, connection, transaction))
        {
            var settings = new NpgsqlConnectionStringBuilder(db.Settings);
            identity.Parameters.AddWithValue("name", db.Name);
            identity.Parameters.AddWithValue("oid", NpgsqlTypes.NpgsqlDbType.Oid, db.Oid.Value);
            identity.Parameters.AddWithValue("owner", NpgsqlTypes.NpgsqlDbType.Oid, db.OwnerOid);
            identity.Parameters.AddWithValue("role", db.Owner);
            identity.Parameters.AddWithValue("host", settings.Host!);
            identity.Parameters.AddWithValue("port", settings.Port);
            var value = await identity.ExecuteScalarAsync();
            if (value is not string json) throw new FixtureSafetyException("IdentityMismatch");
            var result = new SnapshotFingerprint();
            result.Add("DatabaseIdentityAndAcl", key, [json]);
            foreach (string catalog in Catalogs)
            {
                var rows = await Rows(connection, transaction, $"SELECT to_jsonb(t)::text FROM pg_catalog.{catalog} t");
                var normalized = rows.Select(row => NormalizeCatalogRow(catalog, row)).Order(StringComparer.Ordinal);
                result.Add("Catalog." + catalog, key, normalized);
            }
            var tables = new List<(string Name, bool Sequence)>();
            await using (var command = new NpgsqlCommand("""
                SELECT quote_ident(n.nspname)||'.'||quote_ident(c.relname),c.relkind='S'
                FROM pg_catalog.pg_class c JOIN pg_catalog.pg_namespace n ON n.oid=c.relnamespace
                WHERE c.relkind IN ('r','p','m','S') AND n.nspname NOT IN ('pg_catalog','information_schema')
                AND left(n.nspname,3)<>'pg_' ORDER BY n.nspname COLLATE "C",c.relname COLLATE "C"
                """, connection, transaction))
            await using (var reader = await command.ExecuteReaderAsync())
                while (await reader.ReadAsync()) tables.Add((reader.GetString(0), reader.GetBoolean(1)));
            var data = new List<string>();
            var sequences = new List<string>();
            foreach (var table in tables)
            {
                var destination = table.Sequence ? sequences : data;
                destination.Add(table.Name);
                string sql = table.Sequence
                    ? $"SELECT jsonb_build_object('last_value',last_value,'log_cnt',log_cnt,'is_called',is_called)::text FROM {table.Name}"
                    : $"SELECT to_jsonb(t)::text FROM {table.Name} t ORDER BY to_jsonb(t)::text COLLATE \"C\"";
                destination.AddRange(await Rows(connection, transaction, sql));
            }
            result.Add("TableData", key, data);
            result.Add("SequenceState", key, sequences);
            result.Add("LargeObjectContents", key, await Rows(connection, transaction,
                "SELECT oid::text||':'||encode(lo_get(oid),'hex') FROM pg_catalog.pg_largeobject_metadata ORDER BY oid"));
            await transaction.RollbackAsync();
            return result;
        }
    }
}
