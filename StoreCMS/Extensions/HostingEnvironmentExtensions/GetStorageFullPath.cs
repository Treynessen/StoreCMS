using Microsoft.AspNetCore.Hosting;

namespace Treynessen.Extensions
{
    public static partial class HostingEnvironmentExtensions
    {
        private static string storageFolderFullPath;
        public static string GetStorageFolderFullPath(this IHostingEnvironment env)
        {
            if (string.IsNullOrEmpty(storageFolderFullPath))
                storageFolderFullPath = $"{env.ContentRootPath}/Storage/";
            return storageFolderFullPath;
        }
    }
}