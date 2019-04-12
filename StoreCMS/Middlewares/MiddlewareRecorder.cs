using Microsoft.AspNetCore.Builder;

namespace Treynessen.Middlewares
{
    public static class MiddlewareRecorder
    {
        public static IApplicationBuilder AddAccessLevelConfigInItemWhen(this IApplicationBuilder app, string adminPanelPath)
        {
            return app.UseMiddleware<AddAccessLevelConfigInItem>(adminPanelPath);
        }
    }
}