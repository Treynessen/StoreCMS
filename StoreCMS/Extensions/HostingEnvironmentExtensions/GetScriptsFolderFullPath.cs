using Microsoft.AspNetCore.Hosting;

namespace Treynessen.Extensions
{
    public static partial class HostingEnvironmentExtensions
    {
        private static string scriptsFolderFullPath;
        public static string GetScriptsFolderFullPath(this IHostingEnvironment env)
        {
            if (string.IsNullOrEmpty(scriptsFolderFullPath))
                scriptsFolderFullPath = $"{env.GetStorageFolderFullPath()}scripts\\";
            return scriptsFolderFullPath;
        }
    }
}