using System;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Treynessen.Database.Context;

namespace Treynessen
{
    public partial class TimedHostedService : IHostedService, IDisposable
    {
        private void DailyTask(object state)
        {
            using (var scope = serviceScopeFactory.CreateScope())
            {
                CMSDatabase db = scope.ServiceProvider.GetRequiredService<CMSDatabase>();
                TransferLogsFromDbToFiles(db, scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Hosting.IHostingEnvironment>());
                ClearVisitorList(db);
            }
        }
    }
}
