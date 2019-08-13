using Microsoft.AspNetCore.Hosting;

namespace Treynessen.Extensions
{
    public static partial class HostingEnvironmentExtensions
    {
        private static string logsFolderFullPath;
        public static string GetLogsFolderFullPath(this IHostingEnvironment env)
        {
            if (string.IsNullOrEmpty(logsFolderFullPath))
                logsFolderFullPath = $"{env.ContentRootPath}/Logs/";
            return logsFolderFullPath;
        }
    }
}