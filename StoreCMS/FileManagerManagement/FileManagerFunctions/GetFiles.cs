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
        private static KeyValuePair<string, FileManagerObjectType>[] typesOfExtensions =
        {
            new KeyValuePair<string, FileManagerObjectType>(".jpg", FileManagerObjectType.Image),
            new KeyValuePair<string, FileManagerObjectType>(".jpeg", FileManagerObjectType.Image),
            new KeyValuePair<string, FileManagerObjectType>(".png", FileManagerObjectType.Image),
            new KeyValuePair<string, FileManagerObjectType>(".bmp", FileManagerObjectType.Image),
            new KeyValuePair<string, FileManagerObjectType>(".gif", FileManagerObjectType.Image),
            new KeyValuePair<string, FileManagerObjectType>(".webp", FileManagerObjectType.Image),
            new KeyValuePair<string, FileManagerObjectType>(".ico", FileManagerObjectType.Image),
            new KeyValuePair<string, FileManagerObjectType>(".css", FileManagerObjectType.Style),
            new KeyValuePair<string, FileManagerObjectType>(".js", FileManagerObjectType.Script)
        };

        private static IEnumerable<FileManagerObject> GetFiles(string path, IHostingEnvironment env)
        {
            // Без проверки на существование директории path, т.к. этот метод работает в паре с GetFilesAndFolders
            return Directory.GetFiles(path)
            .Where(pathToFile =>
            {
                // Проверяем соответствует ли именование файла требованиям:
                // - название должно быть написано маленькими буквами и содержать только допустимые символы
                // список допустимых символов: qwertyuiopasdfghjklzxcvbnm1234567890-_
                // - файл должен иметь допустимое расширение
                // список допустимых расширений: .jpg, .jpeg, .png, .bmp, .gif, .ico, .css
                bool correctExtension = false;
                foreach (var typeOfExtension in typesOfExtensions)
                {
                    if (pathToFile.EndsWith(typeOfExtension.Key, StringComparison.Ordinal))
                    {
                        correctExtension = true;
                        break;
                    }
                }
                if (!correctExtension)
                    return false;
                string fileName = pathToFile.Substring(path.Length + 1);
                int pointIndex = fileName.LastIndexOf('.');
                for (int i = 0; i < pointIndex; ++i)
                {
                    bool correctSymbol = false;
                    foreach (var symbol in availableSymbolsInName)
                    {
                        if (fileName[i].Equals(symbol))
                        {
                            correctSymbol = true;
                            break;
                        }
                    }
                    if (!correctSymbol)
                        return false;
                }
                return true;
            })
            .Select(pathToFile =>
            {
                string shortPath = pathToFile.Substring(env.GetStorageFolderFullPath().Length).Replace('\\', '>');
                string fileName = shortPath.Substring(shortPath.LastIndexOf('>') + 1);
                string fileExtension = fileName.Substring(fileName.LastIndexOf('.'));
                string name = fileName.Substring(0, fileName.Length - fileExtension.Length);
                FileManagerObject fileManagerObject = new FileManagerObject { Name = name, ShortPath = shortPath, CanDelete = true };
                foreach (var typeOfExtension in typesOfExtensions)
                {
                    if (fileExtension.Equals(typeOfExtension.Key, StringComparison.OrdinalIgnoreCase))
                    {
                        fileManagerObject.Type = typeOfExtension.Value;
                        break;
                    }
                }
                return fileManagerObject;
            });
        }
    }
}