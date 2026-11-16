using System;
using Microsoft.Extensions.Hosting;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Treynessen.Database.Context;

namespace Treynessen
{
    public partial class TimedHostedService : IHostedService, IDisposable
    {
        private void ClearVisitorList(CMSDatabase db, CancellationToken token)
        {
            using var budget = CancellationTokenSource.CreateLinkedTokenSource(token);
            budget.CancelAfter(CleanupWork.PhaseBudget);
            CleanupWork.Visits(VisitorStatistics.Shared,
                cancellation => PostgreSqlCleanupStore.Open(db.Database.GetConnectionString(), cancellation), budget.Token).GetAwaiter().GetResult();
        }
    }
}
