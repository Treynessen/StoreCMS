using System;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Treynessen.Extensions;

namespace Treynessen.Functions
{
    public static partial class OtherFunctions
    {
        public static void DeleteFileOrFolder(string path, HttpContext context)
        {
            Regex regex = new Regex(@"^/((\w|-|_)+/)*((\w|-|_)+\.\w+)?$");
            if (!regex.IsMatch(path))
                return;
            bool isFolder = false;
            path = path.Replace('/', '\\');
            if (path[0].Equals('\\'))
                path = path.Remove(0, 1);
            if (path[path.Length - 1].Equals('\\'))
                isFolder = true;
            IHostingEnvironment env = context.RequestServices.GetService<IHostingEnvironment>();
            string storagePath = env.GetStoragePath();
            path = $"{storagePath}{path}";
            var cannotBeDeleted = env.CannotBeDeletedDirectories();
            if (isFolder)
            {
                if (!Directory.Exists(path))
                    return;
                foreach (var cbd in cannotBeDeleted)
                {
                    if (path.Equals(cbd.Directory, StringComparison.InvariantCulture))
                        return;
                }
                Directory.Delete(path, true);
            }
            else
            {
                if (!File.Exists(path))
                    return;
                string fileDirectory = path.Substring(0, path.LastIndexOf('\\') + 1);
                foreach (var cbd in cannotBeDeleted)
                {
                    if (fileDirectory.Equals(cbd.Directory, StringComparison.InvariantCulture))
                    {
                        if (!cbd.CanDeleteFiles)
                            return;
                        break;
                    }
                }
                string fileExtension = path.Substring(path.LastIndexOf('.'));
                File.Delete(path);
                if (fileExtension.Equals(".jpg", StringComparison.InvariantCultureIgnoreCase)
                || fileExtension.Equals(".jpeg", StringComparison.InvariantCultureIgnoreCase)
                || fileExtension.Equals(".png", StringComparison.InvariantCultureIgnoreCase)
                || fileExtension.Equals(".bmp", StringComparison.InvariantCultureIgnoreCase)
                || fileExtension.Equals(".gif", StringComparison.InvariantCultureIgnoreCase))
                {
                    string imageFullName = path.Substring(fileDirectory.Length);
                    DeleteCreatedImages(fileDirectory, imageFullName);
                    string imagesInfoPath = $"{fileDirectory}images.info";
                    if (File.Exists(imagesInfoPath))
                    {
                        string imagesInfoContent = GetFileContent(imagesInfoPath);
                        regex = new Regex($"name = {imageFullName}; width = \\d+; height = \\d+\n");
                        string match = regex.Match(imagesInfoContent).Value;
                        if (!string.IsNullOrEmpty(match))
                        {
                            ReplaceContentInFile(imagesInfoPath, match, string.Empty, imagesInfoContent);
                            imagesInfoContent = GetFileContent(imagesInfoPath);
                            if (string.IsNullOrEmpty(imagesInfoContent))
                                File.Delete(imagesInfoPath);
                        }
                    }
                }
            }
        }
    }
}