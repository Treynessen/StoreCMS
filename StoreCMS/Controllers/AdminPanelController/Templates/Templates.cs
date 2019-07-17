using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Treynessen.AdminPanelTypes;

namespace Treynessen.Controllers
{
    public partial class AdminPanelController : Controller
    {
        [NonAction]
        public IActionResult Templates()
        {
            HttpContext.Items["pageID"] = AdminPanelPages.Templates;
            return View("Templates/TemplatesPage", db.Templates.AsNoTracking().ToArrayAsync().Result);
        }
    }
}