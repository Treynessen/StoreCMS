using Microsoft.AspNetCore.Hosting;

namespace Treynessen.Extensions
{
    public static partial class HostingEnvironmentExtensions
    {
        private static string chunksFolderFullPath;
        public static string GetChunksFolderFullPath(this IHostingEnvironment env)
        {
            if (string.IsNullOrEmpty(chunksFolderFullPath))
                chunksFolderFullPath = $"{env.ContentRootPath}/Views/Chunks/";
            return chunksFolderFullPath;
        }
    }
}