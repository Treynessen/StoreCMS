using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Treynessen.Database.Entities;

namespace Treynessen
{
    internal interface ICleanupStore : IAsyncDisposable
    {
        Task<(int? Visitors, int? Pages)> VisitBounds(CancellationToken token);
        Task<int> DeletePages(int bound, CancellationToken token);
        Task<int> DeleteVisitors(int bound, CancellationToken token);
        Task<bool> HasVisitors(int bound, CancellationToken token);
        Task<int?> LogBound(CancellationToken token);
        Task<IReadOnlyList<AdminPanelLog>> ReadLogs(int bound, int? after, CancellationToken token);
        Task DeleteLogs(IReadOnlyList<int> ids, CancellationToken token);
    }

    internal static class CleanupWork
    {
        internal const int PageBatch = 1000, VisitorBatch = 100, LogBatch = 100;
        internal const int MaxVisitBatches = 10000, MaxLogBatches = 100;
        internal static readonly TimeSpan PhaseBudget = TimeSpan.FromSeconds(10);
        internal static readonly TimeSpan JobBudget = TimeSpan.FromSeconds(20);
        internal static readonly TimeSpan DrainBudget = TimeSpan.FromSeconds(2);

        internal static async Task Visits(VisitorStatistics admission, Func<CancellationToken, Task<ICleanupStore>> open,
            CancellationToken token)
        {
            using var wait = CancellationTokenSource.CreateLinkedTokenSource(token);
            wait.CancelAfter(DrainBudget);
            using var pause = await admission.PauseAsync(wait.Token).ConfigureAwait(false);
            // [Комментарий ИИ-агента]: Соединение и текущая транзакция освобождаются раньше восстановления допуска.
            await using var store = await open(token).ConfigureAwait(false);
            var bounds = await store.VisitBounds(token).ConfigureAwait(false);
            if (bounds.Pages.HasValue)
                await Drain(t => store.DeletePages(bounds.Pages.Value, t), token).ConfigureAwait(false);
            if (bounds.Visitors.HasValue)
            {
                await Drain(t => store.DeleteVisitors(bounds.Visitors.Value, t), token).ConfigureAwait(false);
                if (await store.HasVisitors(bounds.Visitors.Value, token).ConfigureAwait(false))
                    throw new InvalidOperationException("Остались посетители с зависимыми строками; очистка отложена.");
            }
        }

        private static async Task Drain(Func<CancellationToken, Task<int>> delete, CancellationToken token)
        {
            for (int n = 0; n < MaxVisitBatches; n++)
            {
                token.ThrowIfCancellationRequested();
                if (await delete(token).ConfigureAwait(false) == 0) return;
            }
            throw new InvalidOperationException("Достигнут предел порций; остаток отложен.");
        }

        internal static Task Logs(Func<CancellationToken, Task<ICleanupStore>> open,
            Func<IReadOnlyList<AdminPanelLog>, CancellationToken, IReadOnlyList<int>> publish, CancellationToken token)
            =>Logs(open,publish,token,CleanupObservationContext.Noop);

        internal static async Task Logs(Func<CancellationToken, Task<ICleanupStore>> open,
            Func<IReadOnlyList<AdminPanelLog>, CancellationToken, IReadOnlyList<int>> publish, CancellationToken token,CleanupObservationContext context)
        {
            var observer=CleanupObservationContext.Active(context);
            observer?.Emit(CleanupObservationKind.CallStarted,CleanupComponent.Work,()=>new CleanupEmptyPayload(),token);
            bool completed=false;
            try
            {
            await using var store = await open(token).ConfigureAwait(false);
            int? bound = await store.LogBound(token).ConfigureAwait(false);
            observer?.Emit(CleanupObservationKind.LogBound,CleanupComponent.Work,()=>new CleanupBoundPayload(bound),token);
            if (!bound.HasValue) {completed=true;return;}
            int? after = null;
            for (int n = 0; n < MaxLogBatches; n++)
            {
                token.ThrowIfCancellationRequested();
                observer?.NextBatch();
                var batch = await store.ReadLogs(bound.Value, after, token).ConfigureAwait(false);
                observer?.Emit(CleanupObservationKind.BatchSelected,CleanupComponent.Work,()=>new CleanupIdsPayload(batch.Select(x=>x.ID)),token);
                if (batch.Count == 0) {completed=true;return;}
                observer?.Emit(CleanupObservationKind.PublisherInvocation,CleanupComponent.Work,()=>new CleanupIdsPayload(batch.Select(x=>x.ID)),token);
                var ids = publish(batch, token);
                observer?.Emit(CleanupObservationKind.PublisherReturnedIds,CleanupComponent.Work,()=>new CleanupIdsPayload(ids),token);
                if (ids.Count != batch.Count || !ids.OrderBy(x => x).SequenceEqual(batch.Select(x => x.ID).OrderBy(x => x)))
                    throw new InvalidDataException("Архив не подтвердил точные ID порции.");
                observer?.Emit(CleanupObservationKind.IdsValidated,CleanupComponent.Work,()=>new CleanupIdsPayload(ids),token);
                token.ThrowIfCancellationRequested();
                observer?.Emit(CleanupObservationKind.DeleteInvocation,CleanupComponent.Work,()=>new CleanupIdsPayload(ids),token);
                await store.DeleteLogs(ids, token).ConfigureAwait(false);
                observer?.Emit(CleanupObservationKind.DeleteReturned,CleanupComponent.Work,()=>new CleanupIdsPayload(ids),token);
                after = batch.Max(x => x.ID);
            }
            throw new InvalidOperationException("Достигнут предел архивирования; остаток отложен.");
            }
            catch(Exception error){completed=false;observer?.Failure(CleanupObservationKind.CallFailed,CleanupComponent.Work,error,CleanupCommitKnowledge.Unknown,token);throw;}
            finally{if(completed)observer?.Emit(CleanupObservationKind.CallCompleted,CleanupComponent.Work,()=>new CleanupEmptyPayload(),token);}
        }
    }
}
