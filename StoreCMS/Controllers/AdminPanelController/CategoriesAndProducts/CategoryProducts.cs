using System.Linq;
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
            CategoryPage category = db.CategoryPages.AsNoTracking().FirstOrDefault(c => c.ID == itemID);
            if (category == null)
                return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.Categories}");
            HttpContext.Items["itemID"] = itemID.Value;
            HttpContext.Items["categoryName"] = category.PageName.ToLower();
            return View("CategoriesAndProducts/CategoryProducts", db.ProductPages.AsNoTracking().Where(pp => pp.PreviousPageID == category.ID).ToList());
        }
    }
}