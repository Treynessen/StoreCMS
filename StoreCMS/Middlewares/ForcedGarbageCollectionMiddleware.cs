using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Treynessen.SettingsManagement;

namespace Treynessen.Middlewares
{
    public class ForcedGarbageCollectionMiddleware
    {
        private readonly RequestDelegate next;

        public ForcedGarbageCollectionMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task InvokeAsync(HttpContext context, ConfigurationHandler config)
        {
            Task.Run(() =>
            {
                try
                {
                    int valueToRun = Convert.ToInt32(config.GetConfigValue("ForcedGarbageCollectionSettings:ValueToRun"));
                    if (valueToRun > 0 && GC.GetTotalMemory(false) / 1000000 >= valueToRun)
                    {
                        GC.Collect();
                    }
                }
                catch (FormatException) { }
            });
            await next.Invoke(context);
        }
    }
}