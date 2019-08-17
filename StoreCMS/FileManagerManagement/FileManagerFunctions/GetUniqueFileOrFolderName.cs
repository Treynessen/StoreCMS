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
            bool has = false;
            bool putUnderscore = false;
            int index = 1;
            do
            {
                has = false;
                string current = $"{path}{fileName}{(index == 1 && !putUnderscore ? string.Empty : $"{index.ToString()}")}{fileExtension}";
                foreach (var f in filesOrFolders)
                {
                    if (f.Equals(current, StringComparison.OrdinalIgnoreCase))
                    {
                        has = true;
                        break;
                    }
                }
                if (has && !putUnderscore && index == 1)
                {
                    fileName += "_";
                    putUnderscore = true;
                }
                if (!has)
                {
                    if (!(index == 1 && !putUnderscore))
                        fileName += $"{index.ToString()}";
                    fileName += fileExtension;
                }
                if (index == int.MaxValue)
                {
                    fileName += index.ToString();
                    index = 0;
                }
                ++index;
            } while (has);
            return fileName;
        }
    }
}