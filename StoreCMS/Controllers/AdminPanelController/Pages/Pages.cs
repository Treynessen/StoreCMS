using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Treynessen.AdminPanelTypes;

namespace Treynessen.Controllers
{
    public partial class AdminPanelController : Controller
    {
        [NonAction]
        public IActionResult Pages()
        {
            HttpContext.Items["pageID"] = AdminPanelPages.Pages;
            var usualPages = db.UsualPages.AsNoTracking().OrderBy(p => p.ID).ToArrayAsync().Result;
            return View("Pages/Index", usualPages);
        }
    }
}