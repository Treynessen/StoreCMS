using Microsoft.AspNetCore.Hosting;

namespace Treynessen.Extensions
{
    public static partial class IHostingEnvironmentExtensions
    {
        public static string GetStoragePath(this IHostingEnvironment env, bool shortPath = false)
        {
            if (!shortPath)
                return $"{env.ContentRootPath}\\Storage\\";
            else
                return "/";
        }
    }
}