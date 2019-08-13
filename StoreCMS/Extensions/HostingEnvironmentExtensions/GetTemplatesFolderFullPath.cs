using Microsoft.AspNetCore.Hosting;

namespace Treynessen.Extensions
{
    public static partial class HostingEnvironmentExtensions
    {
        private static string templatesFolderFullPath;
        public static string GetTemplatesFolderFullPath(this IHostingEnvironment env)
        {
            if (string.IsNullOrEmpty(templatesFolderFullPath))
                templatesFolderFullPath = $"{env.ContentRootPath}/Views/Templates/";
            return templatesFolderFullPath;
        }
    }
}