using System;
using System.Threading;
using System.Threading.Tasks;

namespace Treynessen
{
    internal sealed class VisitorStatistics
    {
        internal static readonly VisitorStatistics Shared = new VisitorStatistics();
        private readonly object sync = new object();
        private bool cleaning;
        private int active;
        private TaskCompletionSource drained;

        internal IDisposable TryEnter()
        {
            lock (sync)
            {
                if (cleaning) return null;
                if (active == 0) drained = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
                active++;
                return new Lease(() =>
                {
                    lock (sync) { if (--active == 0) drained.TrySetResult(); }
                });
            }
        }

        internal async Task<IDisposable> PauseAsync(CancellationToken token)
        {
            Task pending;
            lock (sync)
            {
                if (cleaning) throw new InvalidOperationException("Очистка статистики уже выполняется.");
                cleaning = true;
                pending = active == 0 ? Task.CompletedTask : drained.Task;
            }
            var pause = new Lease(() => { lock (sync) cleaning = false; });
            try { await pending.WaitAsync(token).ConfigureAwait(false); token.ThrowIfCancellationRequested(); return pause; }
            catch { pause.Dispose(); throw; }
        }

        private sealed class Lease : IDisposable
        {
            private Action release;
            internal Lease(Action release) { this.release = release; }
            public void Dispose() => Interlocked.Exchange(ref release, null)?.Invoke();
        }
    }
}
