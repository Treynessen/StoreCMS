using Microsoft.AspNetCore.Hosting;

namespace Treynessen.Extensions
{
    public static partial class IHostingEnvironmentExtensions
    {
        public static string GetConfigurationsPath(this IHostingEnvironment env, bool shortPath = false)
        {
            if (shortPath)
            {
                return "~/Configurations/";
            }
            else
            {
                return $"{env.ContentRootPath}\\Configurations\\";
            }
        }
    }
}