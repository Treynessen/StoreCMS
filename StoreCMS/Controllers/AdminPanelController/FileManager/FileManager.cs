using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Treynessen.Extensions;
using Treynessen.Localization;
using Treynessen.AdminPanelTypes;
using Treynessen.FileManagerManagement;

namespace Treynessen.Controllers
{
    public partial class AdminPanelController : Controller
    {
        [NonAction]
        public IActionResult FileManager(string path)
        {
            string fullPath = string.Empty;
            IHostingEnvironment env = HttpContext.RequestServices.GetService<IHostingEnvironment>();
            if (string.IsNullOrEmpty(path))
            {
                fullPath = env.GetStorageFolderFullPath();
            }
            else
            {
                Regex regex = new Regex(@"^((\w|-|_)+)(>(\w|-|_)+)*$");
                if (!regex.IsMatch(path))
                    return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.FileManager}");
                fullPath = env.GetStorageFolderFullPath() + path.Replace('>', '\\').Insert(path.Length, "\\");
            }
            // Проверяем можно ли зайти в текущую директорию
            if(!FileManagerManagementFunctions.HasAccessToFolder(fullPath, env))
                return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.FileManager}");
            // Получаем файлы и папки в текущей директории
            FileManagerObject[] fileManagerObjects = FileManagerManagementFunctions.GetFilesAndFolders(fullPath, env, out bool pathExists);
            if (!pathExists)
                return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.FileManager}");
            HttpContext.Items["pageID"] = AdminPanelPages.FileManager;

            // Получаем навигационную цепочку
            LinkedList<KeyValuePair<string, string>> breadcrumbs = new LinkedList<KeyValuePair<string, string>>();
            // Key - название; Value - путь
            StringBuilder pathBuilder = new StringBuilder();
            breadcrumbs.AddLast(new KeyValuePair<string, string>("/", string.Empty));
            if (!string.IsNullOrEmpty(path))
            {
                string[] subdirectories = path.Split('>');
                for (int i = 0; i < subdirectories.Length; ++i)
                {
                    pathBuilder.Clear();
                    for (int j = 0; j <= i; ++j)
                    {
                        pathBuilder.Append($"{subdirectories[j]}");
                        if (j != i)
                            pathBuilder.Append(">");
                    }
                    breadcrumbs.AddLast(new KeyValuePair<string, string>(subdirectories[i], pathBuilder.ToString()));
                }
            }
            HttpContext.Items["CurrentDirectoryBreadcrumbs"] = breadcrumbs;

            return View("FileManager/Index", fileManagerObjects);
        }
    }
}