using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Treynessen.PagesManagement;
using Treynessen.AdminPanelTypes;
using Treynessen.Database.Entities;

namespace Treynessen.Controllers
{
    public partial class AdminPanelController : Controller
    {
        [NonAction]
        public IActionResult EditCategory(int? itemID)
        {
            HttpContext.Items["pageID"] = AdminPanelPages.EditCategory;
            if (!itemID.HasValue)
                return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.Categories}");
            CategoryPage page = db.CategoryPages.AsNoTracking().FirstOrDefault(up => up.ID == itemID.Value);
            if (page == null)
                return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.Categories}");
            PageModel model = PagesManagementFunctions.PageToPageModel(page);
            HttpContext.Items["UsualPages"] = db.UsualPages.AsNoTracking().ToArray();
            HttpContext.Items["Templates"] = db.Templates.AsNoTracking().ToArray();
            return View("CategoriesAndProducts/EditCategory", model);
        }
    }
}