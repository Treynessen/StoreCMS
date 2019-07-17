using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Treynessen.AdminPanelTypes;

namespace Treynessen.Controllers
{
    public partial class AdminPanelController : Controller
    {
        [NonAction]
        public IActionResult AddProduct(int? itemID)
        {
            HttpContext.Items["pageID"] = AdminPanelPages.AddProduct;
            if (!itemID.HasValue)
                return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.Categories}");
            var categoryPage = db.CategoryPages.FirstOrDefaultAsync(cp => cp.ID == itemID.Value).Result;
            if (categoryPage == null)
                return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.Categories}");
            db.Entry(categoryPage).State = EntityState.Detached;
            HttpContext.Items["CategoryPage"] = categoryPage;
            return View("CategoriesAndProducts/AddProduct");
        }
    }
}