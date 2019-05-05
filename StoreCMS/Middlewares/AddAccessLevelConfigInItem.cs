using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Treynessen.Security;

namespace Treynessen.Middlewares
{
    public class AddAccessLevelConfigInItem
    {
        private RequestDelegate next;
        private string path;

        public AddAccessLevelConfigInItem(RequestDelegate next, string path)
        {
            this.next = next;
            this.path = path.Trim('/');
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path.Value.Trim('/').Equals(path, StringComparison.InvariantCulture))
                context.Items["AccessLevelConfiguration"] = context.RequestServices.GetRequiredService<AccessLevelConfiguration>();
            await next.Invoke(context);
        }
    }
}