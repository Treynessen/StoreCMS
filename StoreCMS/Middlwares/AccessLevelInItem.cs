using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Trane.DependencyInjection;

namespace Trane.Middlwares
{
    public class AccessLevelInItem
    {
        private RequestDelegate next;
        private string adminPanelPath;

        public AccessLevelInItem(RequestDelegate next, string adminPanelPath)
        {
            this.next = next;
            this.adminPanelPath = adminPanelPath;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path.Value.Trim('/').Equals(adminPanelPath, StringComparison.CurrentCulture))
                context.Items["AccessLevelConfiguration"] = context.RequestServices.GetRequiredService<AccessLevelConfiguration>();
            await next.Invoke(context);
        }
    }
}
