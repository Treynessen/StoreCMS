using System;
using System.Threading;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Treynessen
{
    public partial class TimedHostedService : IHostedService, IDisposable
    {
        private IServiceScopeFactory serviceScopeFactory;
        private Timer timer;

        public TimedHostedService(IServiceScopeFactory serviceScopeFactory)
        {
            this.serviceScopeFactory = serviceScopeFactory;
        }
    }
}