using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Treynessen.AdminPanelTypes;

namespace Treynessen.Controllers
{
    public partial class AdminPanelController : Controller
    {
        [NonAction]
        public IActionResult AddPage()
        {
            HttpContext.Items["pageID"] = AdminPanelPages.AddPage;
            HttpContext.Items["UsualPages"] = db.UsualPages.ToArrayAsync().Result;
            HttpContext.Items["Templates"] = db.Templates.ToArrayAsync().Result;
            return View("Pages/AddPage");
        }
    }
}