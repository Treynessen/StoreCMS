using System;
using Microsoft.Extensions.Hosting;

namespace Treynessen
{
    public partial class TimedHostedService : IHostedService, IDisposable
    {
        public void Dispose()
        {
            timer?.Dispose();
        }
    }
}