using Microsoft.AspNetCore.Hosting;

namespace Treynessen.Extensions
{
    public static partial class HostingEnvironmentExtensions
    {
        private static string productsImagesFolderSrc;
        public static string GetProductsImagesFolderSrc(this IHostingEnvironment env)
        {
            if (string.IsNullOrEmpty(productsImagesFolderSrc))
                productsImagesFolderSrc = $"{env.GetImagesFolderSrc()}products/";
            return productsImagesFolderSrc;
        }
    }
}