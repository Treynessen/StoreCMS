using Microsoft.AspNetCore.Hosting;

namespace Treynessen.Extensions
{
    public static partial class HostingEnvironmentExtensions
    {
        private static string stylesFolderFullPath;
        public static string GetStylesFolderFullPath(this IWebHostEnvironment env)
        {
            if (string.IsNullOrEmpty(stylesFolderFullPath))
                stylesFolderFullPath = $"{env.GetStorageFolderFullPath()}styles/";
            return stylesFolderFullPath;
        }
    }
}