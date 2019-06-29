using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Treynessen.AdminPanelTypes;

namespace Treynessen.Controllers
{
    public partial class AdminPanelController : Controller
    {
        [NonAction]
        public IActionResult Categories()
        {
            HttpContext.Items["pageID"] = AdminPanelPages.Categories;
            var categories = db.CategoryPages.AsNoTracking().OrderBy(cp => cp.PageName).ToArrayAsync().Result;
            return View("CategoriesAndProducts/Categories", categories);
        }
    }
}