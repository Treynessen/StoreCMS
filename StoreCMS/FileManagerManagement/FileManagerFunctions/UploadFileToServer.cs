using System;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Treynessen.Functions;
using Treynessen.Extensions;

namespace Treynessen.FileManagerManagement
{
    public static partial class FileManagerManagementFunctions
    {
        public static void UploadFileToServer(string path, IFormFile file, HttpContext context)
        {
            IHostingEnvironment env = context.RequestServices.GetService<IHostingEnvironment>();
            if (string.IsNullOrEmpty(path))
            {
                path = env.GetStorageFolderFullPath();
            }
            else
            {
                Regex regex = new Regex(@"^((\w|-|_)+)(>(\w|-|_)+)*$");
                if (!regex.IsMatch(path))
                    return;
                path = path.Replace('>', '\\');
                if (!path[path.Length - 1].Equals('\\'))
                    path = path.Insert(path.Length, "\\");
                path = $"{env.GetStorageFolderFullPath()}{path}";
            }
            if (!Directory.Exists(path) || !HasAccessToFolder(path, env))
                return;
            int pointIndex = file.FileName.LastIndexOf('.');
            if (pointIndex == -1)
                return;
            string fileExtension = file.FileName.Substring(pointIndex).ToLower();
            bool itsCorrectExtension = false;
            foreach (var typeOfExtension in typesOfExtensions)
            {
                if (fileExtension.Equals(typeOfExtension.Key, StringComparison.InvariantCulture))
                {
                    itsCorrectExtension = true;
                    break;
                }
            }
            if (!itsCorrectExtension)
                return;
            string fileName = file.FileName.Substring(0, pointIndex);
            fileName = OtherFunctions.GetCorrectName(fileName, context);
            if (string.IsNullOrEmpty(fileName))
                fileName = "uploaded_file";
            fileName = GetUniqueFileOrFolderName(path, fileName, fileExtension);
            path += fileName;
            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                file.CopyTo(fs);
            }
        }
    }
}