using Microsoft.AspNetCore.Mvc;
using Treynessen.AdminPanelTypes;

namespace Treynessen.Controllers
{
    public partial class AdminPanelController : Controller
    {
        [NonAction]
        public IActionResult AddTemplate()
        {
            HttpContext.Items["pageID"] = AdminPanelPages.AddTemplate;
            return View("Templates/AddTemplate");
        }
    }
}