using System;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Treynessen.Functions;
using Treynessen.Extensions;
using Treynessen.AdminPanelTypes;

namespace Treynessen.FileManagerManagement
{
    public static partial class FileManagerManagementFunctions
    {
        public static void EditScriptFile(string path, StyleModel model, HttpContext context, out bool successfullyCompleted)
        {
            Regex regex = new Regex(@"^((\w|-|_)+)(>(\w|-|_)+)*\.js$");
            if (!regex.IsMatch(path))
            {
                successfullyCompleted = false;
                return;
            }
            IHostingEnvironment env = context.RequestServices.GetService<IHostingEnvironment>();
            string scriptFileFullName = path.Substring(path.LastIndexOf('>') + 1);
            path = path.Substring(0, path.Length - scriptFileFullName.Length);
            if (!string.IsNullOrEmpty(path))
            {
                path = path.Replace('>', '\\');
                if (!path[path.Length - 1].Equals('\\'))
                    path = path.Insert(path.Length, "\\");
            }
            path = $"{env.GetStorageFolderFullPath()}{path}";
            string pathToFile = path + scriptFileFullName;
            if (!File.Exists(pathToFile) || !HasAccessToFolder(path, env))
            {
                successfullyCompleted = false;
                return;
            }
            model.FileName = OtherFunctions.GetCorrectName(model.FileName, context);
            if (string.IsNullOrEmpty(model.FileName))
            {
                successfullyCompleted = false;
                return;
            }
            string oldScriptFileName = scriptFileFullName.Substring(0, scriptFileFullName.Length - 3);
            string scriptFileFullPath = $"{path}{model.FileName}.js";
            if (!oldScriptFileName.Equals(model.FileName, StringComparison.InvariantCulture))
            {
                File.Move($"{path}{scriptFileFullName}", scriptFileFullPath);
            }
            using (StreamWriter writer = new StreamWriter(scriptFileFullPath))
            {
                writer.Write(model.FileContent);
            }
            successfullyCompleted = true;
        }
    }
}