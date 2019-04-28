using Microsoft.AspNetCore.Hosting;

namespace Treynessen.Extensions
{
    public static partial class IHostingEnvironmentExtensions
    {
        public static string GetImagesPath(this IHostingEnvironment env, bool shortPath = false)
        {
            if (!shortPath)
                return $"{env.GetStoragePath()}images\\";
            else
                return "/images/";
        }
    }
}