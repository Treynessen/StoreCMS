using Microsoft.AspNetCore.Mvc;
using Treynessen.AdminPanelTypes;

namespace Treynessen.Controllers
{
    public partial class AdminPanelController : Controller
    {
        [NonAction]
        public IActionResult Settings()
        {
            HttpContext.Items["pageID"] = AdminPanelPages.Settings;
            return View("Settings");
        }
    }
}