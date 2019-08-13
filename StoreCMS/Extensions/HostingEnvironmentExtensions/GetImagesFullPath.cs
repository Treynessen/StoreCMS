using Microsoft.AspNetCore.Hosting;

namespace Treynessen.Extensions
{
    public static partial class HostingEnvironmentExtensions
    {
        private static string imagesFolderFullPath;
        public static string GetImagesFolderFullPath(this IHostingEnvironment env)
        {
            if (string.IsNullOrEmpty(imagesFolderFullPath))
                imagesFolderFullPath = $"{env.GetStorageFolderFullPath()}images/";
            return imagesFolderFullPath;
        }
    }
}