using System;
using System.IO;

namespace Treynessen.FileManagerManagement
{
    public static partial class FileManagerManagementFunctions
    {
        public static string GetUniqueFileOrFolderName(string path, string fileName, string fileExtension = null)
        {
            string[] filesOrFolders = null;
            try
            {
                if (!string.IsNullOrEmpty(fileExtension))
                    filesOrFolders = Directory.GetFiles(path, $"*{fileExtension}");
                else
                    filesOrFolders = Directory.GetDirectories(path);
            }
            catch (DirectoryNotFoundException) { }
            if (filesOrFolders == null || filesOrFolders.Length == 0)
                return $"{fileName}{fileExtension}";
            bool has = true;
            int index = 0;
            while (has)
            {
                has = false;
                if (index == int.MaxValue)
                {
                    fileName += index.ToString();
                    index = 0;
                }
                string current = $"{path}{fileName}{(index == 0 ? string.Empty : $"{index.ToString()}")}{fileExtension}";
                foreach (var f in filesOrFolders)
                {
                    if (f.Equals(current, StringComparison.OrdinalIgnoreCase))
                    {
                        has = true;
                        break;
                    }
                }
                if (index == 0 && has)
                {
                    fileName += "_";
                }
                if (!has)
                {
                    if (index > 0)
                        fileName += $"{index.ToString()}";
                    fileName += fileExtension;
                }
                ++index;
            }
            return fileName;
        }
    }
}