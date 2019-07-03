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
        public static void CreateCssFile(string path, string fileName, HttpContext context)
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
            fileName = OtherFunctions.GetCorrectName(fileName, context);
            if (string.IsNullOrEmpty(fileName))
                return;
            fileName = GetUniqueFileOrFolderName(path, fileName, ".css");
            File.Create($"{path}{fileName}").Close();
        }
    }
}