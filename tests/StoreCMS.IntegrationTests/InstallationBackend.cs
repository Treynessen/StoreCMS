using System.Reflection;
using Treynessen.Security.Installation;
using Treynessen.Security.Installation.Automatic;
using Treynessen.Security.Installation.Bootstrap;

namespace StoreCMS.IntegrationTests;

internal sealed class InstallationBackend : IAsyncDisposable
{
    // [Комментарий ИИ-агента]: Единственный мост к internal API сохраняет уникальную assembly identity без изменения production visibility.
    private readonly object backend;
    private readonly Type type = typeof(AutomaticPostgreSqlInstallation).Assembly.GetType(
        "Treynessen.Security.Installation.Automatic.NpgsqlAutomaticInstallationBackend", true)!;
    internal InstallationBackend(string settings) => backend = Activator.CreateInstance(type,
        BindingFlags.Instance | BindingFlags.NonPublic, null, [settings], null)!;
    private Task Call(string name, CancellationToken token = default) => (Task)type.GetMethod(name)!.Invoke(backend, [token])!;
    internal async Task<bool> Lock() => await (Task<bool>)Call("LockAsync");
    internal async Task<string> Inspect()
    {
        var task = Call("InspectDatabaseAsync");
        await task;
        return task.GetType().GetProperty("Result")!.GetValue(task)!.ToString()!;
    }
    internal Task Schema(CancellationToken token = default) => Call("CreateSchemaAsync", token);
    internal async Task<BootstrapResult> Bootstrap(string initialPassword) =>
        await (Task<BootstrapResult>)type.GetMethod("BootstrapAsync")!.Invoke(backend, [initialPassword, CancellationToken.None])!;
    public ValueTask DisposeAsync() => ((IAsyncDisposable)backend).DisposeAsync();
}
