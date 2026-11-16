using Microsoft.AspNetCore.Hosting;

namespace Treynessen.Extensions
{
    public static partial class HostingEnvironmentExtensions
    {
        private static string storageFolderFullPath;
        public static string GetStorageFolderFullPath(this IWebHostEnvironment env)
        {
            if (string.IsNullOrEmpty(storageFolderFullPath))
                storageFolderFullPath = $"{env.ContentRootPath}/Storage/";
            return storageFolderFullPath;
        }
    }
}