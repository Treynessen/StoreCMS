using Microsoft.AspNetCore.Hosting;

namespace Treynessen.Extensions
{
    public static partial class HostingEnvironmentExtensions
    {
        private static string productBlockCshtmlFullPath;
        public static string GetProductBlockCshtmlFullPath(this IHostingEnvironment env)
        {
            if (string.IsNullOrEmpty(productBlockCshtmlFullPath))
                productBlockCshtmlFullPath = $"{env.GetConfigsFolderFullPath()}product_block.cshtml";
            return productBlockCshtmlFullPath;
        }
    }
}