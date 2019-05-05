using System;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Treynessen.Functions;
using Treynessen.Extensions;
using Treynessen.AdminPanelTypes;

namespace Treynessen.Controllers
{
    public partial class AdminPanelController : Controller
    {
        [NonAction]
        public IActionResult EditCssFile(string path)
        {
            Regex regex = new Regex(@"^/((\w|-|_)+/)*((\w|-|_)+\.\w+)?$");
            if (!regex.IsMatch(path))
                return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.Files}");
            if (!path.EndsWith(".css", StringComparison.InvariantCultureIgnoreCase))
                return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.Files}");
            IHostingEnvironment env = HttpContext.RequestServices.GetService<IHostingEnvironment>();
            string pathToFile = $"{env.GetStoragePath()}{path.Remove(0, 1).Replace('/', '\\')}";
            if (System.IO.File.Exists(pathToFile))
            {
                string fileName = pathToFile.Substring(pathToFile.LastIndexOf('\\') + 1);
                fileName = fileName.Substring(0, fileName.LastIndexOf('.'));
                StyleModel model = new StyleModel
                {
                    FileName = fileName,
                    FileContent = OtherFunctions.GetFileContent(pathToFile)
                };
                return View("Files/EditCss", model);
            }
            else
            {
                return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.Files}");
            }
        }
    }
}
