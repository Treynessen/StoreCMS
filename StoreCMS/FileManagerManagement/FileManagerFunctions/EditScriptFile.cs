using System;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Treynessen.Functions;
using Treynessen.Extensions;
using Treynessen.Localization;
using Treynessen.LogManagement;
using Treynessen.AdminPanelTypes;
using Treynessen.Database.Context;

namespace Treynessen.FileManagerManagement
{
    public static partial class FileManagerManagementFunctions
    {
        public static void EditScriptFile(CMSDatabase db, string path, StyleModel model, HttpContext context, out string redirectPath, out bool successfullyCompleted)
        {
            Regex regex = new Regex(@"^((\w|-|_)+)(>(\w|-|_)+)*\.js$");
            if (!regex.IsMatch(path))
            {
                successfullyCompleted = false;
                redirectPath = string.Empty;
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
                redirectPath = string.Empty;
                return;
            }
            model.FileName = OtherFunctions.GetCorrectName(model.FileName, context);
            if (string.IsNullOrEmpty(model.FileName))
            {
                successfullyCompleted = false;
                redirectPath = string.Empty;
                return;
            }
            string oldScriptFileName = scriptFileFullName.Substring(0, scriptFileFullName.Length - 3);
            string scriptFileFullPath = $"{path}{model.FileName}.js";
            if (!oldScriptFileName.Equals(model.FileName, StringComparison.InvariantCulture))
            {
                File.Move($"{pathToFile}", scriptFileFullPath);
            }
            using (StreamWriter writer = new StreamWriter(scriptFileFullPath))
            {
                writer.Write(model.FileContent);
            }
            successfullyCompleted = true;
            redirectPath = scriptFileFullPath.Substring(env.GetStorageFolderFullPath().Length).Replace('\\', '>');

            LogManagementFunctions.AddAdminPanelLog(
                db: db,
                context: context,
                info: $"{pathToFile}{(!oldScriptFileName.Equals(model.FileName, StringComparison.InvariantCulture) ? $" -> {scriptFileFullPath}" : string.Empty)}: " +
                $"{(context.Items["LogLocalization"] as IAdminPanelLogLocalization)?.FileEdited}"
            );
        }
    }
}