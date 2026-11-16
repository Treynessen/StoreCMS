using System.Linq;
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
            HttpContext.Items["UsualPages"] = db.UsualPages.AsNoTracking().ToArray();
            HttpContext.Items["Templates"] = db.Templates.AsNoTracking().ToArray();
            return View("CategoriesAndProducts/AddCategory");
        }
    }
}