using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;

namespace Treynessen.FileManagerManagement
{
    public static partial class FileManagerManagementFunctions
    {
        private static string availableSymbolsInName = "qwertyuiopasdfghjklzxcvbnm1234567890-_";

        public static FileManagerObject[] GetFilesAndFolders(string path, IHostingEnvironment env, out bool pathExists)
        {
            if (Directory.Exists(path))
            {
                pathExists = true;
                return GetFolders(path, env).Concat(GetFiles(path, env)).ToArray();
            }
            else
            {
                pathExists = false;
                return null;
            }
        }
    }
}