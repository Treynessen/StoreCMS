using System;
using Microsoft.Extensions.Hosting;

namespace Treynessen
{
    public partial class TimedHostedService : IHostedService, IDisposable
    {
        public void Dispose()
        {
            // [Комментарий ИИ-агента]: Dispose запрещает новые работы и начинает тот же drain, не освобождая активный scope досрочно.
            _ = BeginStop();
        }
    }
}
