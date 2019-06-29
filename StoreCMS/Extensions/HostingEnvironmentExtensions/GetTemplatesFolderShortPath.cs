using Microsoft.AspNetCore.Hosting;

namespace Treynessen.Extensions
{
    public static partial class HostingEnvironmentExtensions
    {
        private static string templatesFolderShortPath;
        public static string GetTemplatesFolderShortPath(this IHostingEnvironment env)
        {
            if (string.IsNullOrEmpty(templatesFolderShortPath))
                templatesFolderShortPath = "~/Views/Templates/";
            return templatesFolderShortPath;
        }
    }
}