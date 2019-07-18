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
        public static void CreateFolder(string path, string folderName, HttpContext context, out bool successfullyCreated)
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
                {
                    successfullyCreated = false;
                    return;
                }
                path = path.Replace('>', '\\');
                if (!path[path.Length - 1].Equals('\\'))
                    path = path.Insert(path.Length, "\\");
                path = $"{env.GetStorageFolderFullPath()}{path}";
            }
            if (!Directory.Exists(path) || !HasAccessToFolder(path, env))
            {
                successfullyCreated = false;
                return;
            }
            folderName = OtherFunctions.GetCorrectName(folderName, context);
            if (string.IsNullOrEmpty(folderName))
            {
                successfullyCreated = false;
                return;
            }
            folderName = GetUniqueFileOrFolderName(path, folderName);
            Directory.CreateDirectory($"{path}{folderName}");
            successfullyCreated = true;
        }
    }
}