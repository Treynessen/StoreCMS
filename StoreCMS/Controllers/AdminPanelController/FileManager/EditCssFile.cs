using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Treynessen.Functions;
using Treynessen.Extensions;
using Treynessen.AdminPanelTypes;
using Treynessen.FileManagerManagement;

namespace Treynessen.Controllers
{
    public partial class AdminPanelController : Controller
    {
        [NonAction]
        public IActionResult EditCssFile(string path)
        {
            Regex regex = new Regex(@"^((\w|-|_)+)(>(\w|-|_)+)*\.css$");
            if (!regex.IsMatch(path))
                return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.FileManager}");
            IHostingEnvironment env = HttpContext.RequestServices.GetService<IHostingEnvironment>();
            HttpContext.Items["pageID"] = AdminPanelPages.EditStyle;
            string cssFileFullName = path.Substring(path.LastIndexOf('>') + 1);
            path = path.Substring(0, path.Length - cssFileFullName.Length);
            if (!string.IsNullOrEmpty(path))
            {
                path = path.Replace('>', '\\');
                if (!path[path.Length - 1].Equals('\\'))
                    path = path.Insert(path.Length, "\\");
            }
            path = $"{env.GetStorageFolderFullPath()}{path}";
            string pathToFile = path + cssFileFullName;
            if (!System.IO.File.Exists(pathToFile) || !FileManagerManagementFunctions.HasAccessToFolder(path, env))
                return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.FileManager}");
            string cssFileContent = OtherFunctions.GetFileContent(pathToFile);
            StyleModel model = new StyleModel { FileName = cssFileFullName.Substring(0, cssFileFullName.Length - 4), FileContent = cssFileContent };
            return View("FileManager/EditCssFile", model);
        }
    }
}