using System.Collections.Concurrent;
using System.Diagnostics.Tracing;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace StoreCMS.UnitTests.TestDoubles;

public abstract class NetworkIsolatedTest : IDisposable
{
    private readonly NetworkAudit audit = new();

    public void Dispose()
    {
        audit.Dispose();
        Assert.Empty(audit.Attempts);
        GC.SuppressFinalize(this);
    }

    private sealed class NetworkAudit : EventListener
    {
        internal ConcurrentQueue<string> Attempts { get; } = new();

        protected override void OnEventSourceCreated(EventSource source)
        {
            if (source.Name is "System.Net.Sockets" or "System.Net.NameResolution")
                EnableEvents(source, EventLevel.Verbose);
        }

        protected override void OnEventWritten(EventWrittenEventArgs data)
        {
            if (data.EventName is "ConnectStart" or "AcceptStart" or "ResolutionStart")
                Attempts.Enqueue(data.EventName);
        }
    }
}
