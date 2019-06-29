using Microsoft.AspNetCore.Hosting;

namespace Treynessen.Extensions
{
    public static partial class HostingEnvironmentExtensions
    {
        private static string imagesFolderSrc;
        public static string GetImagesFolderSrc(this IHostingEnvironment env)
        {
            if (string.IsNullOrEmpty(imagesFolderSrc))
                imagesFolderSrc = "/images/";
            return imagesFolderSrc;
        }
    }
}