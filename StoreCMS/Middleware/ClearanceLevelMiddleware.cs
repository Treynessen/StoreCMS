using Trane.Configurations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Trane.Middleware
{
    public class ClearanceLevelMiddleware
    {
        private RequestDelegate next;
        private string adminPanelPath;

        public ClearanceLevelMiddleware(RequestDelegate next, string adminPanelPath)
        {
            this.next = next;
            this.adminPanelPath = adminPanelPath;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path.Value.Trim('/') == adminPanelPath)
                context.Items["ClearanceLevelConfiguration"] = context.RequestServices.GetRequiredService<ClearanceLevelConfiguration>();
            await next.Invoke(context);
        }
    }
}
