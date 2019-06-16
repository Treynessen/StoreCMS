using Microsoft.AspNetCore.Hosting;

namespace Treynessen.Extensions
{
    public class FolderInfo
    {
        public string Directory { get; }
        public bool CanDeleteFiles{ get; }

        public FolderInfo(string directory, bool canDeleteFiles)
        {
            Directory = directory;
            CanDeleteFiles = canDeleteFiles;
        }
    }

    public static partial class IHostingEnvironmentExtensions
    {
        public static FolderInfo[] CannotBeDeletedDirectories(this IHostingEnvironment env)
        {
            string storagePath = GetStoragePath(env);
            return new FolderInfo[]
            {
                new FolderInfo($"{storagePath}", true),
                new FolderInfo($"{storagePath}images\\", true),
                new FolderInfo($"{storagePath}images\\products\\", false),
                new FolderInfo($"{storagePath}styles\\", true),
                new FolderInfo($"{storagePath}styles\\admin_panel\\", false),
                new FolderInfo($"{storagePath}styles\\admin_panel\\files\\", false),
                new FolderInfo($"{storagePath}styles\\admin_panel\\pages\\", false),
                new FolderInfo($"{storagePath}styles\\admin_panel\\products\\", false),
                new FolderInfo($"{storagePath}styles\\admin_panel\\templates\\", false)
            };
        }
    }
}