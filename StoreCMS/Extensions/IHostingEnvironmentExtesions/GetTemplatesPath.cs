using Microsoft.AspNetCore.Hosting;

namespace Treynessen.Extensions
{
    public static partial class IHostingEnvironmentExtensions
    {
        public static string GetTemplatesPath(this IHostingEnvironment env, bool shortPath = false)
        {
            if (!shortPath)
                return $"{env.ContentRootPath}\\Views\\Templates\\";
            else
                return "~/Views/Templates/";
        }
    }
}