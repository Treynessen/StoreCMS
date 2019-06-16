using System;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Treynessen.Extensions;
using Treynessen.AdminPanelTypes;

namespace Treynessen.Functions
{
    public static partial class OtherFunctions
    {
        public static bool EditCssFile(string path, StyleModel model, HttpContext context, out string newPath)
        {
            newPath = null;
            Regex regex = new Regex(@"^((\w|-|_)+)(@(\w|-|_)+)*(\w|-|_)+.css$");
            if (!regex.IsMatch(path))
                return false;
            IHostingEnvironment env = context.RequestServices.GetService<IHostingEnvironment>();
            string pathToFile = $"{env.GetStoragePath()}{path.Replace('@', '\\')}";
            string oldFullFileName = path.Substring(path.LastIndexOf('@') + 1);
            model.FileName = GetCorrectName(model.FileName, context);
            if (string.IsNullOrEmpty(model.FileName))
                return false;
            string newFullFileName = $"{model.FileName}.css";
            if (!oldFullFileName.Equals(newFullFileName, StringComparison.InvariantCultureIgnoreCase))
            {
                string pathToFileDirectory = pathToFile.Substring(0, pathToFile.LastIndexOf('\\') + 1);
                newFullFileName = GetUniqueFileOrFolderName(pathToFileDirectory, model.FileName, ".css");
                File.Delete(pathToFile);
                pathToFile = $"{pathToFileDirectory}{newFullFileName}";
            }
            newPath = path.Replace(oldFullFileName, newFullFileName);
            using (StreamWriter writer = new StreamWriter(pathToFile, false))
            {
                writer.Write(model.FileContent);
            }
            return true;
        }
    }
}
