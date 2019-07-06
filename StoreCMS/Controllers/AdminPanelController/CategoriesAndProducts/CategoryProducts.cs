using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Treynessen.AdminPanelTypes;
using Treynessen.Database.Entities;

namespace Treynessen.Controllers
{
    public partial class AdminPanelController : Controller
    {
        [NonAction]
        public IActionResult CategoryProducts(int? itemID)
        {
            HttpContext.Items["pageID"] = AdminPanelPages.CategoryProducts;
            if (!itemID.HasValue)
                return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.Categories}");
            CategoryPage category = db.CategoryPages.FirstOrDefaultAsync(c => c.ID == itemID).Result;
            if (category == null)
                return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.Categories}");
            db.Entry(category).Collection(c => c.ProductPages).LoadAsync().Wait();
            HttpContext.Items["itemID"] = itemID.Value;
            HttpContext.Items["categoryName"] = category.PageName.ToLower();
            return View("CategoriesAndProducts/CategoryProducts", category.ProductPages);
        }
    }
}