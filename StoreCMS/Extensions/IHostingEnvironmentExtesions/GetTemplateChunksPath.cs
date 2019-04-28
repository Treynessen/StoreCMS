using Microsoft.AspNetCore.Hosting;

namespace Treynessen.Extensions
{
    public static partial class IHostingEnvironmentExtensions
    {
        public static string GetTemplateChunksPath(this IHostingEnvironment env, bool shortPath = false)
        {
            if (!shortPath)
                return $"{env.ContentRootPath}\\Views\\TemplateChunks\\";
            else
                return "~/Views/TemplateChunks/";
        }
    }
}