﻿using System;
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
        public static void EditCssFile(CMSDatabase db, string path, StyleModel model, HttpContext context, out string redirectPath, out bool successfullyCompleted)
        {
            Regex regex = new Regex(@"^((\w|-|_)+)(>(\w|-|_)+)*\.css$");
            if (!regex.IsMatch(path))
            {
                successfullyCompleted = false;
                redirectPath = string.Empty;
                return;
            }
            IHostingEnvironment env = context.RequestServices.GetService<IHostingEnvironment>();
            string cssFileFullName = path.Substring(path.LastIndexOf('>') + 1);
            path = path.Substring(0, path.Length - cssFileFullName.Length);
            if (!string.IsNullOrEmpty(path))
            {
                path = path.Replace('>', '/');
                if (!path[path.Length - 1].Equals('/'))
                    path = path.Insert(path.Length, "/");
            }
            path = $"{env.GetStorageFolderFullPath()}{path}";
            string pathToFile = path + cssFileFullName;
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
            string oldCssFileName = cssFileFullName.Substring(0, cssFileFullName.Length - 4);
            string cssFileFullPath = $"{path}{model.FileName}.css";
            if (!oldCssFileName.Equals(model.FileName, StringComparison.Ordinal))
            {
                File.Move($"{pathToFile}", cssFileFullPath);
            }
            using (StreamWriter writer = new StreamWriter(cssFileFullPath))
            {
                writer.Write(model.FileContent);
            }
            successfullyCompleted = true;
            redirectPath = cssFileFullPath.Substring(env.GetStorageFolderFullPath().Length).Replace('/', '>');

            LogManagementFunctions.AddAdminPanelLog(
                db: db,
                context: context,
                info: $"{pathToFile.Substring(env.GetStorageFolderFullPath().Length - 1)}{(!oldCssFileName.Equals(model.FileName, StringComparison.Ordinal) ? $" -> {cssFileFullPath.Substring(env.GetStorageFolderFullPath().Length - 1)}" : string.Empty)}: " +
                $"{(context.Items["LogLocalization"] as IAdminPanelLogLocalization)?.FileEdited}"
            );
        }
    }
}