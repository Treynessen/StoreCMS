using Microsoft.AspNetCore.Builder;

namespace Trane.Middlwares
{
    public static class MiddlewareRecorder
    {
        public static IApplicationBuilder UseAccessLevelInItem(this IApplicationBuilder app, string adminPanelPath)
        {
            return app.UseMiddleware<AccessLevelInItem>(adminPanelPath);
        }
    }
}
