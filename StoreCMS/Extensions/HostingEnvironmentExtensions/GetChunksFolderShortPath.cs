using Microsoft.AspNetCore.Hosting;

namespace Treynessen.Extensions
{
    public static partial class HostingEnvironmentExtensions
    {
        private static string chunksFolderShortPath;
        public static string GetChunksFolderShortPath(this IHostingEnvironment env)
        {
            if (string.IsNullOrEmpty(chunksFolderShortPath))
                chunksFolderShortPath = "~/Views/Chunks/";
            return chunksFolderShortPath;
        }
    }
}