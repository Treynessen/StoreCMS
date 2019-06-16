using System;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Treynessen.Extensions;

namespace Treynessen.Functions
{
    public static partial class OtherFunctions
    {
        public static void UploadFileToServer(string path, IFormFile file, HttpContext context)
        {
            string fullFileName = file.FileName;
            int index = fullFileName.LastIndexOf('.');
            if (index == -1)
                return;
            string fileExtension = fullFileName.Substring(index);
            if (!fileExtension.Equals(".jpg", StringComparison.InvariantCultureIgnoreCase)
                && !fileExtension.Equals(".jpeg", StringComparison.InvariantCultureIgnoreCase)
                && !fileExtension.Equals(".png", StringComparison.InvariantCultureIgnoreCase)
                && !fileExtension.Equals(".bmp", StringComparison.InvariantCultureIgnoreCase)
                && !fileExtension.Equals(".gif", StringComparison.InvariantCultureIgnoreCase)
                && !fileExtension.Equals(".css", StringComparison.InvariantCultureIgnoreCase)
                && !fileExtension.Equals(".ico", StringComparison.InvariantCultureIgnoreCase))
                return;
            string fileName = fullFileName.Substring(0, index);
            fileName = GetCorrectName(fileName, context);
            fullFileName = $"{fileName}{fileExtension}";
            IHostingEnvironment env = context.RequestServices.GetService<IHostingEnvironment>();
            if (string.IsNullOrEmpty(path))
                path = env.GetStoragePath();
            else
            {
                path = path.Replace('@', '\\');
                if (!path[path.Length - 1].Equals('\\'))
                    path = path.Insert(path.Length, "\\");
                path = $"{env.GetStoragePath()}{path}";
            }
            path = $"{path}{GetUniqueFileOrFolderName(path, fileName, fileExtension)}";
            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                file.CopyTo(fs);
            }
        }
    }
}