using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Treynessen.Extensions;
using Treynessen.Localization;
using Treynessen.AdminPanelTypes;

namespace Treynessen.Controllers
{
    public partial class AdminPanelController : Controller
    {
        // Если directory == string.Empty, тогда считать, что сделан запрос к корневой папке
        [NonAction]
        public IActionResult Files(string directory = null)
        {
            Regex regex = new Regex(@"^((\w|-|_)+)(@(\w|-|_)+)*$");
            if (!string.IsNullOrEmpty(directory) && !regex.IsMatch(directory))
                return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.Files}");
            IHostingEnvironment env = HttpContext.RequestServices.GetService<IHostingEnvironment>();
            // Получение полного пути до хранилища
            string storagePath = env.GetStoragePath();
            if (string.IsNullOrEmpty(directory) || directory.Equals("/", StringComparison.InvariantCulture))
                directory = storagePath;
            else
            {
                directory = directory.Replace('@', '\\');
                directory = $"{storagePath}{directory}";
                if (!directory[directory.Length - 1].Equals('\\'))
                    directory = directory.Insert(directory.Length, "\\");
            }
            if (!Directory.Exists(directory))
                return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.Files}");
            string availableSymbols = "qwertyuiopasdfghjklzxcvbnm1234567890-_";
            // Список директорий, к которым нельзя получить доступ из админ-панели
            string[] forbiddenDirectories = new string[]
            {
                $"{storagePath}images\\products\\",
                $"{storagePath}styles\\admin_panel\\"
            };
            if (!directory.Equals(storagePath, StringComparison.InvariantCulture))
            {
                foreach (var fd in forbiddenDirectories)
                {
                    if (directory.Contains(fd, StringComparison.InvariantCultureIgnoreCase))
                        return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.Files}");
                }
                int startIndex = storagePath.Length;
                int endIndex = directory.Length - 1;
                string[] folderNames = directory.Substring(startIndex, endIndex - startIndex).Split('\\');
                // Проверяем путь до текущей директории. Если встречаются недопустимые символы, то делаем редирект
                // в корень хранилища
                foreach (var fn in folderNames)
                {
                    for (int i = 0; i < fn.Length; ++i)
                    {
                        if (!availableSymbols.Contains(fn[i], StringComparison.InvariantCultureIgnoreCase))
                            return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.Files}");
                    }
                }
            }

            IFilesPageLocalization localization = HttpContext.RequestServices.GetService<IFilesPageLocalization>();
            HttpContext.Items["FilesPageLocalization"] = localization;

            IEnumerable<FilesViewModel> files = Directory.GetFiles(directory)
            .Where(f =>
            {
                if (!f.EndsWith(".jpg", StringComparison.InvariantCultureIgnoreCase)
                && !f.EndsWith(".jpeg", StringComparison.InvariantCultureIgnoreCase)
                && !f.EndsWith(".png", StringComparison.InvariantCultureIgnoreCase)
                && !f.EndsWith(".bmp", StringComparison.InvariantCultureIgnoreCase)
                && !f.EndsWith(".gif", StringComparison.InvariantCultureIgnoreCase)
                && !f.EndsWith(".css", StringComparison.InvariantCultureIgnoreCase)
                && !f.EndsWith(".ico", StringComparison.InvariantCultureIgnoreCase))
                    return false;
                bool wasPoint = false;
                int startIndex = directory.Length;
                int endIndex = f.Length - 1;
                for (int i = startIndex; i <= endIndex; ++i)
                {
                    if (!wasPoint && f[i].Equals('.'))
                        wasPoint = true;
                    else if (!availableSymbols.Contains(f[i], StringComparison.InvariantCultureIgnoreCase))
                        return false;
                }
                return true;
            })
            .Select(f =>
            {
                FilesViewModel fvm = new FilesViewModel
                {
                    Name = f.Substring(directory.Length),
                    CanDelete = true
                };
                if (f.EndsWith(".jpg", StringComparison.InvariantCultureIgnoreCase)
                || f.EndsWith(".jpeg", StringComparison.InvariantCultureIgnoreCase)
                || f.EndsWith(".png", StringComparison.InvariantCultureIgnoreCase)
                || f.EndsWith(".bmp", StringComparison.InvariantCultureIgnoreCase)
                || f.EndsWith(".gif", StringComparison.InvariantCultureIgnoreCase)
                || f.EndsWith(".ico", StringComparison.InvariantCultureIgnoreCase))
                    fvm.FileType = FileTypeEnum.image;
                else fvm.FileType = FileTypeEnum.style;
                if (fvm.FileType == FileTypeEnum.image)
                    fvm.Url = f.Substring(storagePath.Length).Replace('\\', '/');
                else fvm.Url = f.Substring(storagePath.Length).Replace('\\', '@');
                return fvm;
            });

            var cannotBeDeleted = env.CannotBeDeletedDirectories();

            IEnumerable<FilesViewModel> subdirectories = Directory.GetDirectories(directory)
            .Where(d =>
            {
                if (!d[d.Length - 1].Equals('\\'))
                    d = d.Insert(d.Length, "\\");
                foreach (var fd in forbiddenDirectories)
                {
                    if (d.Contains(fd, StringComparison.InvariantCulture))
                        return false;
                }
                int startIndex = directory.Length;
                int endIndex = d.Length - 1;
                for (int i = startIndex; i < endIndex; ++i)
                {
                    if (!availableSymbols.Contains(d[i], StringComparison.InvariantCultureIgnoreCase))
                        return false;
                }
                return true;
            })
            .Select(d =>
            {
                if (d[d.Length - 1].Equals('\\'))
                    d = d.Substring(0, d.Length - 1);
                string url = d.Substring(storagePath.Length).Replace('\\', '@');
                int index = url.LastIndexOf('@', url.Length - 1) + 1;
                string name = url.Substring(index, url.Length - index);
                FilesViewModel fvm = new FilesViewModel
                {
                    Name = name,
                    Url = url
                };
                d = d.Insert(d.Length, "\\");
                foreach (var cbd in cannotBeDeleted)
                {
                    if (d.Equals(cbd.Directory, StringComparison.InvariantCulture))
                    {
                        fvm.CanDelete = false;
                        break;
                    }
                }
                if (!fvm.CanDelete.HasValue)
                    fvm.CanDelete = true;
                return fvm;
            });

            string[] folders = directory.Substring(storagePath.Length - 1, directory.Length - storagePath.Length).Split('\\');
            LinkedList<FilesViewModel> breadcrumbs = new LinkedList<FilesViewModel>();
            for (int i = 0; i < folders.Length; ++i)
            {
                if (i == 0)
                    breadcrumbs.AddLast(new FilesViewModel { Name = "/", Url = "" });
                else
                    breadcrumbs.AddLast(new FilesViewModel { Name = folders[i], Url = $"{(!string.IsNullOrEmpty(folders[i - 1]) ? $"{folders[i-1]}@" : string.Empty)}{folders[i]}" });
            }
            HttpContext.Items["FilesPageBreadcrumbs"] = breadcrumbs;

            FilesViewModel[] model = subdirectories.Concat(files).ToArray();
            return View("Files/Index", model);
        }
    }
}
