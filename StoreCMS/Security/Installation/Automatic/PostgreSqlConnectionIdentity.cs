using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;

namespace Treynessen.Security.Installation.Automatic;

internal static class PostgreSqlConnectionIdentity
{
    internal sealed record ActualConnectionIdentity(string Database, string User, string SessionUser, IPAddress Host,
        int Port, int Major, string Encoding, string Schema);
    internal static bool Matches(NpgsqlConnectionStringBuilder expected, ActualConnectionIdentity actual) =>
        IPAddress.TryParse(expected.Host, out var host) && actual.Database == expected.Database
        && actual.User == expected.Username && actual.SessionUser == expected.Username && actual.Host.Equals(host)
        && actual.Port == expected.Port && actual.Major == 18 && actual.Encoding == "UTF8" && actual.Schema == "public";
    internal static async Task VerifyAsync(NpgsqlConnection connection, NpgsqlTransaction transaction, CancellationToken token)
    {
        var expected = new NpgsqlConnectionStringBuilder(connection.ConnectionString);
        if (!IPAddress.TryParse(expected.Host, out var host) || string.IsNullOrWhiteSpace(expected.Username)
            || string.IsNullOrWhiteSpace(expected.Database)) throw new InvalidOperationException("ExplicitEndpointRequired");
        await using var command = new NpgsqlCommand("""
            SELECT current_database(),current_user,session_user,inet_server_addr(),inet_server_port(),
                current_setting('server_version_num')::int/10000,current_setting('server_encoding'),current_schema()
            """, connection, transaction);
        await using var reader = await command.ExecuteReaderAsync(token);
        if (!await reader.ReadAsync(token) || !Matches(expected, new ActualConnectionIdentity(
            reader.GetString(0), reader.GetString(1), reader.GetString(2), reader.GetFieldValue<IPAddress>(3),
            reader.GetInt32(4), reader.GetInt32(5), reader.GetString(6), reader.GetString(7))))
            throw new InvalidOperationException("ConnectionIdentityMismatch");
    }
}
