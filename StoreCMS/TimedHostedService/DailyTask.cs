using System;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using Treynessen.Database.Context;

namespace Treynessen
{
    public partial class TimedHostedService : IHostedService, IDisposable
    {
        private void DailyTask(object state)
        {
            TaskCompletionSource work;
            lock (lifecycleLock)
            {
                if (stopping || activeWork != null) return;
                // [Комментарий ИИ-агента]: Допуск callback и его учёт атомарны относительно перехода в stopping.
                activeWork = work = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            }
            try
            {
                using var jobBudget = CancellationTokenSource.CreateLinkedTokenSource(cleanupCancellation.Token);
                jobBudget.CancelAfter(CleanupWork.JobBudget);
                using (var scope = serviceScopeFactory.CreateScope())
                {
                    CMSDatabase db = scope.ServiceProvider.GetRequiredService<CMSDatabase>();
                    Exception archiveFailure = null;
                    try { TransferLogsFromDbToFiles(db, scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>(), jobBudget.Token); }
                    catch (Exception error) { archiveFailure = error; }
                    try { ClearVisitorList(db, jobBudget.Token); }
                    catch (Exception error)
                    {
                        if (archiveFailure != null) throw new AggregateException(archiveFailure, error);
                        throw;
                    }
                    if (archiveFailure != null) System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(archiveFailure).Throw();
                }
            }
            catch (Exception error)
            {
                ReportFailure(error, "Ежедневная задача не завершена");
            }
            finally
            {
                // [Комментарий ИИ-агента]: Scope и аварийная диагностика завершаются до сигнала об окончании работы.
                lock (lifecycleLock)
                {
                    activeWork = null;
                    work.SetResult();
                }
            }
        }

        private void ReportFailure(Exception error, string operation)
        {
            Interlocked.Exchange(ref lastFailure, error);
            try
            {
                using (var scope = serviceScopeFactory.CreateScope())
                    scope.ServiceProvider.GetRequiredService<ILogger<TimedHostedService>>()
                        .LogError(error, "{Operation}: {ErrorType}", operation, error.GetType().Name);
            }
            catch (Exception loggingError)
            {
                Exception failure = new AggregateException(error, loggingError);
                Interlocked.Exchange(ref lastFailure, failure);
                try
                {
                    // [Комментарий ИИ-агента]: Fallback не раскрывает тексты исключений; при отказе обоих каналов причина остаётся в LastFailure.
                    Console.Error.WriteLine("StoreCMS: {0} ({1}); ошибка журналирования ({2}).",
                        operation, error.GetType().FullName, loggingError.GetType().FullName);
                }
                catch (Exception fallbackError)
                {
                    Interlocked.Exchange(ref lastFailure, new AggregateException(failure, fallbackError));
                }
            }
        }
    }
}
