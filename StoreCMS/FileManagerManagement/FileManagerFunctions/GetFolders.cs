using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Treynessen.Extensions;

namespace Treynessen.FileManagerManagement
{
    public static partial class FileManagerManagementFunctions
    {
        private static IEnumerable<FileManagerObject> GetFolders(string path, IHostingEnvironment env)
        {
            // Без проверки на существование директории path, т.к. этот метод работает в паре с GetFilesAndFolders
            return Directory.GetDirectories(path)
            .Where(directory =>
            {
                if (!directory[directory.Length - 1].Equals('\\'))
                    directory = directory.Insert(directory.Length, "\\");
                return HasAccessToFolder(directory, env);
            })
            .Select(directory =>
            {
                string shortPathToFolder = directory.Substring(env.GetStorageFolderFullPath().Length);
                if (shortPathToFolder[shortPathToFolder.Length - 1].Equals('\\'))
                    shortPathToFolder = shortPathToFolder.Substring(0, shortPathToFolder.Length - 1);
                string folderName = shortPathToFolder.Substring(shortPathToFolder.LastIndexOf('\\') + 1);
                shortPathToFolder = shortPathToFolder.Replace('\\', '>');
                bool canDelete = true;
                if (!directory[directory.Length - 1].Equals('\\'))
                    directory = directory.Insert(directory.Length, "\\");
                foreach (var directoryInfo in env.GetStorageDirectoriesInfo())
                {
                    if (directory.Equals(directoryInfo.Path, StringComparison.InvariantCulture))
                    {
                        canDelete = false;
                        break;
                    }
                }
                return new FileManagerObject { Name = folderName, ShortPath = shortPathToFolder, Type = FileManagerObjectType.Folder, CanDelete = canDelete };
            });
        }
    }
}