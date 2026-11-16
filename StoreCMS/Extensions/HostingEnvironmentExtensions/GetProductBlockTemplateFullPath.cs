using Microsoft.AspNetCore.Hosting;

namespace Treynessen.Extensions
{
    public static partial class HostingEnvironmentExtensions
    {
        private static string productBlockTemplateFullPath;
        public static string GetProductBlockTemplateFullPath(this IWebHostEnvironment env)
        {
            if (string.IsNullOrEmpty(productBlockTemplateFullPath))
                productBlockTemplateFullPath = $"{env.GetConfigsFolderFullPath()}product_block.template";
            return productBlockTemplateFullPath;
        }
    }
}