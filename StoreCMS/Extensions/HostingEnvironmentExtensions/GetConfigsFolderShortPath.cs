using Microsoft.AspNetCore.Hosting;

namespace Treynessen.Extensions
{
    public static partial class HostingEnvironmentExtensions
    {
        private static string configsFolderShortPath;
        public static string GetConfigsFolderShortPath(this IHostingEnvironment env)
        {
            if (string.IsNullOrEmpty(configsFolderShortPath))
                configsFolderShortPath = "~/Configs/";
            return configsFolderShortPath;
        }
    }
}