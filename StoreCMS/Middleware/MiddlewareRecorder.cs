using Microsoft.AspNetCore.Builder;

namespace Trane.Middleware
{
    public static class MiddlewareRecorder
    {
        public static IApplicationBuilder UseClearanceLevelMiddleware(this IApplicationBuilder app, string adminPanelPath)
        {
            return app.UseMiddleware<ClearanceLevelMiddleware>(adminPanelPath);
        }
    }
}
