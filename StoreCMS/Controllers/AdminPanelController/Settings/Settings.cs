using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Treynessen.AdminPanelTypes;

namespace Treynessen.Controllers
{
    public partial class AdminPanelController : Controller
    {
        [NonAction]
        public IActionResult Settings()
        {
            HttpContext.Items["pageID"] = AdminPanelPages.Settings;
            HttpContext.Items["Templates"] = db.Templates.AsNoTracking().ToArrayAsync().Result;
            return View("Settings");
        }
    }
}