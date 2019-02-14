using Trane.Localization;
using Microsoft.AspNetCore.Mvc;

namespace Trane.ErrorHandlers
{
    [Area("page404")]
    public class ErrorController : Controller
    {
        private string viewFilePath = "";

        public IActionResult Index([FromServices] ILocalization localization)
        {
            HttpContext.Response.StatusCode = 404;
            if (System.IO.File.Exists(viewFilePath))
                return View(viewFilePath);
            return Content(localization.PageNotFound);
        }
    }
}
