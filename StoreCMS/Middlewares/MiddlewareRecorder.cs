using Microsoft.AspNetCore.Builder;

namespace Treynessen.Middlewares
{
    public static class MiddlewareRecorder
    {
        public static IApplicationBuilder AddAccessLevelConfigInItemWhen(this IApplicationBuilder app, string path)
        {
            return app.UseMiddleware<AddAccessLevelConfigInItem>(path);
        }
    }
}