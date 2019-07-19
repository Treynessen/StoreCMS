using Microsoft.AspNetCore.Hosting;

namespace Treynessen.Extensions
{
    public static partial class HostingEnvironmentExtensions
    {
        private static string productBlockCshtmlShortPath;
        public static string GetProductBlockCshtmlShortPath(this IHostingEnvironment env)
        {
            if (string.IsNullOrEmpty(productBlockCshtmlShortPath))
                productBlockCshtmlShortPath = "~/Settings/product_block.cshtml";
            return productBlockCshtmlShortPath;
        }
    }
}