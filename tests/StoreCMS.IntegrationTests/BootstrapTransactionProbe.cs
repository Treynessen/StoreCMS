using System.Reflection;
using Npgsql;
using Treynessen.Security.Installation;
using Treynessen.Security.Installation.Bootstrap;
using Xunit;

namespace StoreCMS.IntegrationTests;

internal static class BootstrapTransactionProbe
{
    internal static async Task FailAndRollback(string settings, string database, string owner)
    {
        var assembly = typeof(PostgreSqlSecurityBootstrap).Assembly;
        var backendType = assembly.GetType("Treynessen.Security.Installation.Bootstrap.NpgsqlBootstrapBackend", true)!;
        var backend = Activator.CreateInstance(backendType, BindingFlags.Instance | BindingFlags.NonPublic,
            null, [settings, new InstallationTarget(database, owner), true, null], null)!;
        var begin = (Task)backendType.GetMethod("BeginAsync")!.Invoke(backend, [CancellationToken.None])!;
        await begin;
        var transaction = begin.GetType().GetProperty("Result")!.GetValue(begin)!;
        var type = transaction.GetType();
        var planType = assembly.GetType("Treynessen.Security.Installation.Bootstrap.BootstrapPlan", true)!;
        var plan = planType.GetMethod("Create", BindingFlags.Static | BindingFlags.NonPublic)!.Invoke(null, [DateTime.UtcNow])!;
        try
        {
            var insert = (Task<int>)type.GetMethod("InsertRoleAsync")!.Invoke(transaction, [plan, CancellationToken.None])!;
            Assert.True(await insert > 0);
            var failure = (Task<int>)type.GetMethod("InsertUserAsync")!.Invoke(transaction, [plan, int.MaxValue, CancellationToken.None])!;
            var error = await Assert.ThrowsAsync<PostgresException>(async () => await failure);
            Assert.Equal("23503", error.SqlState);
            await (Task)type.GetMethod("RollbackAsync")!.Invoke(transaction, [CancellationToken.None])!;
        }
        finally { await ((IAsyncDisposable)transaction).DisposeAsync(); }
    }
}
