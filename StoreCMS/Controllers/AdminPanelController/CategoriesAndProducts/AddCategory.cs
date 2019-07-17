using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Treynessen.AdminPanelTypes;

namespace Treynessen.Controllers
{
    public partial class AdminPanelController : Controller
    {
        [NonAction]
        public IActionResult AddCategory()
        {
            HttpContext.Items["pageID"] = AdminPanelPages.AddCategory;
            HttpContext.Items["UsualPages"] = db.UsualPages.ToArrayAsync().Result;
            HttpContext.Items["Templates"] = db.Templates.ToArrayAsync().Result;
            return View("CategoriesAndProducts/AddCategory");
        }
    }
}