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
        public static void CreateFolder(string path, string folderName, HttpContext context)
        {
            Regex regex = new Regex(@"^(((\w|-|_)+)(@(\w|-|_)+)*)?((\w|-|_)+)*$");
            if (!string.IsNullOrEmpty(path) && !regex.IsMatch(path))
                return;
            IHostingEnvironment env = context.RequestServices.GetService<IHostingEnvironment>();
            if (string.IsNullOrEmpty(path))
                path = env.GetStoragePath();
            else
                path = $"{env.GetStoragePath()}{path.Replace('@', '\\')}";
            if (!path[path.Length - 1].Equals('\\'))
                path = path.Insert(path.Length, "\\");
            if (!Directory.Exists(path))
                return;
            folderName = GetCorrectName(folderName, context);
            if (string.IsNullOrEmpty(folderName))
                return;
            folderName = GetUniqueFileOrFolderName(path, folderName);
            Directory.CreateDirectory($"{path}{folderName}");
        }
    }
}