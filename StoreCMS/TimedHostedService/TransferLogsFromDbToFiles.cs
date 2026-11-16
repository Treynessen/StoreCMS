using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Treynessen.Extensions;
using Treynessen.LogManagement;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;

namespace Treynessen
{
    public partial class TimedHostedService : IHostedService, IDisposable
    {
        private void TransferLogsFromDbToFiles(CMSDatabase db, Microsoft.AspNetCore.Hosting.IWebHostEnvironment env, CancellationToken token)
        {
            using var budget = CancellationTokenSource.CreateLinkedTokenSource(token);
            budget.CancelAfter(CleanupWork.PhaseBudget);
            var publications = new Dictionary<string, int>();
            CleanupWork.Logs(cancellation => PostgreSqlCleanupStore.Open(db.Database.GetConnectionString(), cancellation), (batch, cancellation) =>
            {
                var published = new List<int>();
                foreach (var group in batch.GroupBy(log => log.UserId))
                {
                    var user = new User { ID = group.Key, AdminPanelLogs = group.ToList() };
                    published.AddRange(LogManagementFunctions.PublishUserLogs(user, env.GetLogsFolderFullPath(), publications, cancellation));
                }
                return published;
            }, budget.Token).GetAwaiter().GetResult();
        }
    }
}
