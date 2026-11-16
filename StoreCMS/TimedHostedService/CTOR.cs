using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Treynessen
{
    public partial class TimedHostedService : IHostedService, IDisposable
    {
        private readonly IServiceScopeFactory serviceScopeFactory;
        private readonly TimeProvider timeProvider;
        private readonly object lifecycleLock = new object();
        private ITimer timer;
        private bool started;
        private bool stopping;
        private TaskCompletionSource activeWork;
        private Task<Exception> shutdown;
        private Exception lastFailure;
        private readonly CancellationTokenSource cleanupCancellation = new CancellationTokenSource();

        public Exception LastFailure => Volatile.Read(ref lastFailure);

        public TimedHostedService(IServiceScopeFactory serviceScopeFactory)
            : this(serviceScopeFactory, TimeProvider.System) { }

        internal TimedHostedService(IServiceScopeFactory serviceScopeFactory, TimeProvider timeProvider)
        {
            this.serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
            this.timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        }
    }
}
