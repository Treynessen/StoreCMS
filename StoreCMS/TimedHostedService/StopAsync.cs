using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Treynessen
{
    public partial class TimedHostedService : IHostedService, IDisposable
    {
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            Exception error = await BeginStop().WaitAsync(cancellationToken).ConfigureAwait(false);
            if (error != null)
                throw new AggregateException("Не удалось полностью остановить таймер фоновой службы.", error);
        }

        internal Task<Exception> DrainBeforeHostDisposalAsync() => BeginStop();

        private Task<Exception> BeginStop()
        {
            lock (lifecycleLock)
            {
                if (shutdown != null) return shutdown;
                stopping = true;
                shutdown = DrainAsync(timer, activeWork?.Task ?? Task.CompletedTask);
                return shutdown;
            }
        }

        private async Task<Exception> DrainAsync(ITimer source, Task work)
        {
            // [Комментарий ИИ-агента]: Остановка и диагностика не блокируют вызывающего до установки отменяемого ожидания.
            await Task.CompletedTask.ConfigureAwait(ConfigureAwaitOptions.ForceYielding);
            Exception failure = null;
            try { await cleanupCancellation.CancelAsync().ConfigureAwait(false); }
            catch (Exception error) { failure = error; ReportFailure(error, "Не удалось отменить очистку"); }
            if (source != null)
            {
                try
                {
                    if (!source.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan))
                        throw new InvalidOperationException("Таймер отклонил остановку.");
                }
                catch (Exception error)
                {
                    failure = failure == null ? error : new AggregateException(failure, error);
                    ReportFailure(error, "Не удалось отключить таймер");
                }
                try { await source.DisposeAsync().ConfigureAwait(false); }
                catch (Exception error)
                {
                    failure = failure == null ? error : new AggregateException(failure, error);
                    ReportFailure(error, "Не удалось освободить таймер");
                }
            }
            // [Комментарий ИИ-агента]: Даже при отказе таймера учтённая работа владеет scope до фактического выхода.
            await work.ConfigureAwait(false);
            lock (lifecycleLock) { timer = null; }
            return failure;
        }
    }
}
