using System;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Treynessen.Database.Context;

namespace Treynessen.Middlewares
{
    public class SitemapMiddleware
    {
        private readonly RequestDelegate next;

        public SitemapMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task InvokeAsync(HttpContext context, CMSDatabase db)
        {
            if (context.Request.Path.Equals("/sitemap.xml", StringComparison.Ordinal))
            {
                StringBuilder sitemapBuilder = new StringBuilder("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">\n");
                foreach(var requestPath in db.UsualPages.AsNoTracking().Where(up => up.Published && up.IsIndex).Select(up => up.RequestPath).ToArray())
                {
                    sitemapBuilder.Append($"\t<url>\n\t\t<loc>{context.Request.Scheme}://{context.Request.Host}{requestPath}</loc>\n\t</url>\n");
                }
                foreach (var requestPath in db.CategoryPages.AsNoTracking().Where(cp => cp.Published && cp.IsIndex).Select(cp => cp.RequestPath).ToArray())
                {
                    sitemapBuilder.Append($"\t<url>\n\t\t<loc>{context.Request.Scheme}://{context.Request.Host}{requestPath}</loc>\n\t</url>\n");
                }
                foreach (var requestPath in db.ProductPages.AsNoTracking().Where(pp => pp.Published && pp.IsIndex).Select(pp => pp.RequestPath).ToArray())
                {
                    sitemapBuilder.Append($"\t<url>\n\t\t<loc>{context.Request.Scheme}://{context.Request.Host}{requestPath}</loc>\n\t</url>\n");
                }
                sitemapBuilder.Append("</urlset>");
                await context.Response.WriteAsync(sitemapBuilder.ToString());
            }
            else await next.Invoke(context);
        }
    }
}