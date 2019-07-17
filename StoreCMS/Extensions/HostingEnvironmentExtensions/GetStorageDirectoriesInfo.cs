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
                    new DirectoryInfo($"{GetStorageFolderFullPath(env)}images\\", true, true),
                    new DirectoryInfo($"{GetStorageFolderFullPath(env)}images\\products\\", false, false),
                    new DirectoryInfo($"{GetStorageFolderFullPath(env)}styles\\", true, true),
                    new DirectoryInfo($"{GetStorageFolderFullPath(env)}styles\\admin_panel\\", false, false),
                    new DirectoryInfo($"{GetStorageFolderFullPath(env)}scripts\\", true, true),
                    new DirectoryInfo($"{GetStorageFolderFullPath(env)}scripts\\admin_panel\\", false, false)
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