using System.IO;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Treynessen.Functions;
using Treynessen.Extensions;
using Treynessen.Localization;
using Treynessen.LogManagement;
using Treynessen.Database.Context;

namespace Treynessen.FileManagerManagement
{
    public static partial class FileManagerManagementFunctions
    {
        public static void CreateCssFile(CMSDatabase db, string path, string fileName, HttpContext context, out bool successfullyCreated)
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
            fileName = OtherFunctions.GetCorrectName(fileName, context);
            if (string.IsNullOrEmpty(fileName))
            {
                successfullyCreated = false;
                return;
            }
            fileName = GetUniqueFileOrFolderName(path, fileName, ".css");
            File.Create($"{path}{fileName}").Close();
            successfullyCreated = true;

            LogManagementFunctions.AddAdminPanelLog(
                db: db,
                context: context,
                info: $"{fileName}: {(context.Items["LogLocalization"] as IAdminPanelLogLocalization)?.FileCreatedIn} {path}"
            );
        }
    }
}