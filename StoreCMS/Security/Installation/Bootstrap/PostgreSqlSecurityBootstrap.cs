using System;
using System.Threading;
using System.Threading.Tasks;

namespace Treynessen.Security.Installation.Bootstrap;

public sealed class PostgreSqlSecurityBootstrap
{
    private readonly AtomicBootstrapCoordinator coordinator;
    public PostgreSqlSecurityBootstrap(string connectionString, InstallationTarget expectedTarget)
    {
        coordinator = new(new NpgsqlBootstrapBackend(connectionString, expectedTarget), expectedTarget, () => DateTime.UtcNow);
    }
    // [Комментарий ИИ-агента]: Вызов требует явного разрешения доверенного локального вызывающего кода; сервис не подключён к Program, MVC или фоновым задачам.
    public Task<BootstrapResult> InstallAsync(InitialInstallationPermission permission = InitialInstallationPermission.Denied,
        CancellationToken cancellationToken = default, string initialPassword = null) =>
        coordinator.InstallAsync(permission, cancellationToken, initialPassword);
}
