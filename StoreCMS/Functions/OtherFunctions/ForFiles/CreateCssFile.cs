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
        public static void CreateCssFile(string path, string fileName, HttpContext context)
        {
            Regex regex = new Regex(@"^(((\w|-|_)+)(@(\w|-|_)+)*)?$");
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
            fileName = GetCorrectName(fileName, context);
            if (string.IsNullOrEmpty(fileName))
                return;
            fileName = GetUniqueFileOrFolderName(path, fileName, ".css");
            File.Create($"{path}{fileName}").Close();
        }
    }
}