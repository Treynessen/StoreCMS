using Microsoft.AspNetCore.Hosting;

namespace Treynessen.Extensions
{
    public static partial class HostingEnvironmentExtensions
    {
        private static DirectoryInfo[] storageDirectoriesInfo;

        public static DirectoryInfo[] GetStorageDirectoriesInfo(this IHostingEnvironment env)
        {
            if (storageDirectoriesInfo == null)
            {
                storageDirectoriesInfo = new DirectoryInfo[]
                {
                    new DirectoryInfo(GetStorageFolderFullPath(env), true, true),
                    new DirectoryInfo(GetImagesFolderFullPath(env), true, true),
                    new DirectoryInfo(GetProductsImagesFolderFullPath(env), false, false),
                    new DirectoryInfo(GetStylesFolderFullPath(env), true, true),
                    new DirectoryInfo($"{GetStylesFolderFullPath(env)}admin_panel\\", false, false),
                    new DirectoryInfo(GetScriptsFolderFullPath(env), true, true),
                    new DirectoryInfo($"{GetScriptsFolderFullPath(env)}admin_panel\\", false, false)
                };
            }
            return storageDirectoriesInfo;
        }
    }

    public class DirectoryInfo
    {
        public string Path { get; } // путь от папки Storage
        public bool CanOpen { get; }
        public bool CanDeleteFilesAndDirectories { get; }

        public DirectoryInfo(string path, bool canOpen, bool canDeleteFilesAndDirectories)
        {
            Path = path;
            CanOpen = canOpen;
            CanDeleteFilesAndDirectories = canDeleteFilesAndDirectories;
        }
    }
}