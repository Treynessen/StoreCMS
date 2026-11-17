using System.Net;
using System.Runtime.InteropServices;
using Npgsql;
using Xunit;

namespace StoreCMS.IntegrationTests;

public sealed class PostgreSqlFactAttribute : FactAttribute
{
    public PostgreSqlFactAttribute()
    {
        if (Environment.GetEnvironmentVariable("STORECMS_T013_ENABLED") != "1")
            Skip = "NOT CONFIGURED: требуется явная конфигурация STORECMS_T013_ENABLED=1; PostgreSQL не подключается.";
    }
}

internal static class PostgreSqlConfiguration
{
    internal static bool Enabled => Environment.GetEnvironmentVariable("STORECMS_T013_ENABLED") == "1";
    internal static NpgsqlConnectionStringBuilder Read()
    {
        string Required(string name) => Environment.GetEnvironmentVariable("STORECMS_T013_" + name)
            ?? throw new InvalidOperationException("TestConfigurationIncomplete");
        string host = Required("HOST");
        if (!IPAddress.TryParse(host, out var address) || !IPAddress.IsLoopback(address)
            || !int.TryParse(Required("PORT"), out int port) || port is < 1 or > 65535
            || Required("PURPOSE") != "disposable-postgres18")
            throw new InvalidOperationException("ExplicitLocalTestEndpointRequired");
        string user = Required("USER");
        string password;
        if (OperatingSystem.IsWindows()) password = ReadCredential(Required("CREDENTIAL_TARGET"), user);
        else
        {
            // [Комментарий ИИ-агента]: Linux получает секрет из унаследованного анонимного pipe, без файла, argv и environment с паролем.
            using var pipe = new System.IO.Pipes.AnonymousPipeClientStream(System.IO.Pipes.PipeDirection.In, Required("PASSWORD_PIPE"));
            using var reader = new StreamReader(pipe);
            password = reader.ReadToEnd();
            if (string.IsNullOrEmpty(password) || password.Length > 4096) throw new InvalidOperationException("TestCredentialInvalid");
        }
        return new()
        {
            Host = host, Port = port, Username = user, Password = password, Database = "postgres",
            Pooling = false, Multiplexing = false, Timeout = 5, CommandTimeout = 10,
            IncludeErrorDetail = false, LogParameters = false, ApplicationName = "StoreCMS.T013.Provisioner"
        };
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct Credential
    {
        public uint Flags, Type;
        public string TargetName, Comment;
        public System.Runtime.InteropServices.ComTypes.FILETIME LastWritten;
        public uint Size;
        public IntPtr Blob;
        public uint Persist, Count;
        public IntPtr Attributes;
        public string Alias, User;
    }
    [DllImport("advapi32", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool CredReadW(string target, uint type, uint flags, out IntPtr value);
    [DllImport("advapi32")]
    private static extern void CredFree(IntPtr value);
    private static string ReadCredential(string target, string user)
    {
        if (!CredReadW(target, 1, 0, out var pointer)) throw new InvalidOperationException("TestCredentialUnavailable");
        try
        {
            var value = Marshal.PtrToStructure<Credential>(pointer);
            if (value.User != user || value.Size == 0 || value.Size % 2 != 0)
                throw new InvalidOperationException("TestCredentialIdentityMismatch");
            return Marshal.PtrToStringUni(value.Blob, checked((int)value.Size / 2))!;
        }
        finally { CredFree(pointer); }
    }
}
