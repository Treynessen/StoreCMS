using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Treynessen
{
    public partial class TimedHostedService : IHostedService, IDisposable
    {
        public Task StartAsync(CancellationToken cancellationToken)
        {
            DateTime nextDay = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day).AddDays(1);
            timer = new Timer(DailyTask, null, nextDay - DateTime.Now, new TimeSpan(24, 0, 0));
            return Task.CompletedTask;
        }
    }
}