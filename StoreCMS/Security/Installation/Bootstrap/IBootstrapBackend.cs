using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Treynessen.Security.Installation.Bootstrap;

internal interface IBootstrapBackend
{
    Task<InstallationResult> InspectAsync(CancellationToken token);
    Task<IBootstrapTransaction> BeginAsync(CancellationToken token);
}

internal interface IBootstrapTransaction : IAsyncDisposable
{
    Task<InstallationIdentity> ReadIdentityAsync(CancellationToken token);
    Task<bool> HasLockPermissionAsync(CancellationToken token);
    Task<bool> TryAdvisoryLockAsync(CancellationToken token);
    Task LockTablesAsync(IReadOnlyList<string> orderedTables, CancellationToken token);
    Task<InstallationResult> ValidateAfterLocksAsync(CancellationToken token);
    Task<int> InsertRoleAsync(BootstrapPlan plan, CancellationToken token);
    Task<int> InsertUserAsync(BootstrapPlan plan, int roleId, CancellationToken token);
    Task InsertMarkerAsync(BootstrapPlan plan, int userId, int roleId, CancellationToken token);
    Task<InstallationData> ReadDataAsync(CancellationToken token);
    Task CommitAsync(CancellationToken token);
    Task RollbackAsync(CancellationToken token);
}
