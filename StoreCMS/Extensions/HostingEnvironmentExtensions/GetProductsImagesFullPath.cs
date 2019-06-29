using Microsoft.AspNetCore.Hosting;

namespace Treynessen.Extensions
{
    public static partial class HostingEnvironmentExtensions
    {
        private static string productsImagesFolderFullPath;
        public static string GetProductsImagesFolderFullPath(this IHostingEnvironment env)
        {
            if (string.IsNullOrEmpty(productsImagesFolderFullPath))
                productsImagesFolderFullPath = $"{env.GetImagesFolderFullPath()}products\\";
            return productsImagesFolderFullPath;
        }
    }
}