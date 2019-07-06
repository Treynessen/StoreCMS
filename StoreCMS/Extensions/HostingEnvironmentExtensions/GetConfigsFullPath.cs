using Microsoft.AspNetCore.Hosting;

namespace Treynessen.Extensions
{
    public static partial class HostingEnvironmentExtensions
    {
        private static string configsFolderFullPath;
        public static string GetConfigsFolderFullPath(this IHostingEnvironment env)
        {
            if (string.IsNullOrEmpty(configsFolderFullPath))
                configsFolderFullPath = $"{env.ContentRootPath}\\Settings\\";
            return configsFolderFullPath;
        }
    }
}