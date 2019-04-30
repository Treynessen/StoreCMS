using Microsoft.AspNetCore.Hosting;

namespace Treynessen.Extensions
{
    public static partial class IHostingEnvironmentExtensions
    {
        public static string GetProductsImagesPath(this IHostingEnvironment env, bool shortPath = false)
        {
            if (!shortPath)
                return $"{env.GetImagesPath()}products\\";
            else
                return $"{env.GetImagesPath(true)}products/";
        }
    }
}