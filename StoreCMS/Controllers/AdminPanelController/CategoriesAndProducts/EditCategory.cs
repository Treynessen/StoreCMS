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
            CategoryPage page = db.CategoryPages.FirstOrDefaultAsync(up => up.ID == itemID.Value).Result;
            if (page == null)
                return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.Categories}");
            db.Entry(page).State = EntityState.Detached;
            PageModel model = PagesManagementFunctions.PageToPageModel(page);
            HttpContext.Items["UsualPages"] = db.UsualPages.ToArrayAsync().Result;
            HttpContext.Items["Templates"] = db.Templates.ToArrayAsync().Result;
            return View("CategoriesAndProducts/EditCategory", model);
        }
    }
}