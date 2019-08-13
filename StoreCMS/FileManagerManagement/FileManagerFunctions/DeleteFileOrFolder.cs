using System;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Treynessen.Extensions;
using Treynessen.Localization;
using Treynessen.LogManagement;
using Treynessen.ImagesManagement;
using Treynessen.Database.Context;

namespace Treynessen.FileManagerManagement
{
    public static partial class FileManagerManagementFunctions
    {
        public static void DeleteFileOrFolder(CMSDatabase db, string path, HttpContext context, out string redirectPath)
        {
            IHostingEnvironment env = context.RequestServices.GetService<IHostingEnvironment>();
            Regex regex = new Regex(@"^((\w|-|_)+)(>(\w|-|_)+)*(\.\w+)?$");
            if (!regex.IsMatch(path))
            {
                redirectPath = null;
                return;
            }
            string fileOrFolderFullName = path.Substring(path.LastIndexOf('>') + 1);
            redirectPath = path = path.Substring(0, path.Length - fileOrFolderFullName.Length);
            if (!string.IsNullOrEmpty(path))
            {
                path = path.Replace('>', '/');
                if (!path[path.Length - 1].Equals('/'))
                    path = path.Insert(path.Length, "/");
                if (redirectPath[redirectPath.Length - 1].Equals('>'))
                    redirectPath = redirectPath.Substring(0, redirectPath.Length - 1);
            }
            path = $"{env.GetStorageFolderFullPath()}{path}";
            if (!Directory.Exists(path) || !HasAccessToFolder(path, env))
            {
                redirectPath = null;
                return;
            }
            FileManagerObjectType? type = null;
            int pointIndex = fileOrFolderFullName.LastIndexOf('.');
            if (pointIndex == -1)
                type = FileManagerObjectType.Folder;
            else
            {
                string fileExtension = fileOrFolderFullName.Substring(pointIndex);
                foreach (var typeOfExtension in typesOfExtensions)
                {
                    if (fileExtension.Equals(typeOfExtension.Key))
                    {
                        type = typeOfExtension.Value;
                        break;
                    }
                }
                if (!type.HasValue)
                {
                    redirectPath = null;
                    return;
                }
                for (int i = 0; i < pointIndex; ++i)
                {
                    bool correctSymbol = false;
                    foreach (var symbol in availableSymbolsInName)
                    {
                        if (fileOrFolderFullName[i].Equals(symbol))
                        {
                            correctSymbol = true;
                            break;
                        }
                    }
                    if (!correctSymbol)
                    {
                        redirectPath = null;
                        return;
                    }
                }
            }
            if (type != FileManagerObjectType.Folder)
            {
                string pathToFile = $"{path}{fileOrFolderFullName}";
                if (File.Exists(pathToFile))
                {
                    if (type == FileManagerObjectType.Image)
                    {
                        ImagesManagementFunctions.DeleteImage(path, fileOrFolderFullName, db, env);
                    }
                    else File.Delete(pathToFile);
                }
                else redirectPath = null;
                LogManagementFunctions.AddAdminPanelLog(
                    db: db,
                    context: context,
                    info: $"{pathToFile.Substring(env.GetStorageFolderFullPath().Length - 1)}: {(context.Items["LogLocalization"] as IAdminPanelLogLocalization)?.FileDeleted}"
                );
            }
            else
            {
                string pathToFolder = $"{path}{fileOrFolderFullName}/";
                if (!Directory.Exists(pathToFolder))
                {
                    redirectPath = null;
                    return;
                }
                foreach (var dir in env.GetStorageDirectoriesInfo())
                {
                    if (pathToFolder.Equals(dir.Path, StringComparison.OrdinalIgnoreCase))
                    {
                        redirectPath = null;
                        return;
                    }
                }
                Directory.Delete(pathToFolder, true);
                LogManagementFunctions.AddAdminPanelLog(
                    db: db,
                    context: context,
                    info: $"{pathToFolder.Substring(env.GetStorageFolderFullPath().Length - 1)}: {(context.Items["LogLocalization"] as IAdminPanelLogLocalization)?.FolderDeleted}"
                );
            }
        }
    }
}