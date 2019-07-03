using System;
using Microsoft.AspNetCore.Hosting;
using Treynessen.Extensions;

namespace Treynessen.FileManagerManagement
{
    public static partial class FileManagerManagementFunctions
    {
        public static bool HasAccessToFolder(string fullPath, IHostingEnvironment env)
        {
            DirectoryInfo[] directories = env.GetStorageDirectoriesInfo();
            foreach(var dir in directories)
            {
                if (fullPath.Contains(dir.Path, StringComparison.InvariantCultureIgnoreCase) && !dir.CanOpen)
                    return false;
            }
            return true;
        }
    }
}