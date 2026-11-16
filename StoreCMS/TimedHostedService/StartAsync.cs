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
            cancellationToken.ThrowIfCancellationRequested();
            lock (lifecycleLock)
            {
                if (stopping || started)
                    throw new InvalidOperationException("Повторный запуск фоновой службы запрещён.");
                DateTime now = timeProvider.GetLocalNow().DateTime;
                timer = timeProvider.CreateTimer(DailyTask, null, now.Date.AddDays(1) - now, TimeSpan.FromHours(24));
                started = true;
            }
            return Task.CompletedTask;
        }
    }
}
