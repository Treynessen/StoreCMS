using Microsoft.AspNetCore.Builder;

namespace Treynessen.Middlewares
{
    public static class IApplicationBuilderExtenstions
    {
        public static IApplicationBuilder UseForcedGarbageCollection (this IApplicationBuilder app)
        {
            return app.UseMiddleware<ForcedGarbageCollectionMiddleware>();
        }
        public static IApplicationBuilder UseSitemap(this IApplicationBuilder app)
        {
            return app.UseMiddleware<SitemapMiddleware>();
        }
    }
}