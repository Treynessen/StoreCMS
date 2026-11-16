using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Treynessen.Hosting;

internal sealed class CleanupHost : IHost, IAsyncDisposable
{
    private readonly IHost inner;
    private readonly TimedHostedService cleanup;
    private readonly object sync = new object();
    private Task start;
    private Task<Exception> stop;
    private Task<Exception> disposal;

    private CleanupHost(IHost inner, TimedHostedService cleanup)
    {
        this.inner = inner;
        this.cleanup = cleanup;
    }

    internal static CleanupHost Create(IHost inner)
    {
        ArgumentNullException.ThrowIfNull(inner);
        try { return new CleanupHost(inner, inner.Services.GetRequiredService<TimedHostedService>()); }
        catch (Exception creationError)
        {
            try { inner.Dispose(); }
            catch (Exception disposalError) { throw new AggregateException(creationError, disposalError); }
            throw;
        }
    }

    public IServiceProvider Services => inner.Services;

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        lock (sync)
        {
            if (start != null || stop != null) throw new InvalidOperationException("Повторный запуск host запрещён.");
            return start = StartCoreAsync(cancellationToken);
        }
    }

    private async Task StartCoreAsync(CancellationToken token)
    {
        await Task.CompletedTask.ConfigureAwait(ConfigureAwaitOptions.ForceYielding);
        await inner.StartAsync(token).ConfigureAwait(false);
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        Exception failure = await BeginStop(cancellationToken).ConfigureAwait(false);
        if (failure != null) System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(failure).Throw();
    }

    private Task<Exception> BeginStop(CancellationToken token)
    {
        lock (sync)
        {
            if (stop != null) return stop;
            // [Комментарий ИИ-агента]: Запрет callback устанавливается синхронно; host владеет drain даже после отмены штатного ожидания службы.
            Task<Exception> drained = cleanup.DrainBeforeHostDisposalAsync();
            return stop = StopCoreAsync(start, drained, token);
        }
    }

    private async Task<Exception> StopCoreAsync(Task starting, Task<Exception> drained, CancellationToken token)
    {
        await Task.CompletedTask.ConfigureAwait(ConfigureAwaitOptions.ForceYielding);
        Exception failure = null;
        if (starting != null)
        {
            try { await starting.ConfigureAwait(false); }
            catch (Exception error) { failure = error; }
        }
        try { await inner.StopAsync(token).ConfigureAwait(false); }
        catch (Exception error) { failure = Combine(failure, error); }
        // [Комментарий ИИ-агента]: Timeout отменяет graceful shutdown, но не даёт права освобождать зависимости активного callback. Неотменяемая операция может удерживать это ожидание без конечного срока.
        return Combine(failure, await drained.ConfigureAwait(false));
    }

    private Task<Exception> BeginDispose()
    {
        lock (sync)
            return disposal ??= DisposeCoreAsync(BeginStop(CancellationToken.None));
    }

    private async Task<Exception> DisposeCoreAsync(Task<Exception> stopped)
    {
        await Task.CompletedTask.ConfigureAwait(ConfigureAwaitOptions.ForceYielding);
        Exception failure = await stopped.ConfigureAwait(false);
        try
        {
            if (inner is IAsyncDisposable asyncDisposable)
                await asyncDisposable.DisposeAsync().ConfigureAwait(false);
            else inner.Dispose();
        }
        catch (Exception error) { failure = Combine(failure, error); }
        return failure;
    }

    public void Dispose()
    {
        Exception failure = BeginDispose().GetAwaiter().GetResult();
        if (failure != null) System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(failure).Throw();
    }

    public async ValueTask DisposeAsync()
    {
        Exception failure = await BeginDispose().ConfigureAwait(false);
        if (failure != null) System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(failure).Throw();
    }

    private static Exception Combine(Exception first, Exception second) =>
        first == null ? second : second == null ? first : new AggregateException(first, second);
}
