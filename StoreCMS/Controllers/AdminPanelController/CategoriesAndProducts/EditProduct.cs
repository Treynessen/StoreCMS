using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Treynessen.AdminPanelTypes;
using Treynessen.PagesManagement;
using Treynessen.Database.Entities;

namespace Treynessen.Controllers
{
    public partial class AdminPanelController : Controller
    {
        [NonAction]
        public IActionResult EditProduct(int? itemID, PageModel model = null)
        {
            HttpContext.Items["pageID"] = AdminPanelPages.EditProduct;
            if (!itemID.HasValue)
                return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.Categories}");
            if (model == null)
            {
                ProductPage page = db.ProductPages.FirstOrDefaultAsync(pp => pp.ID == itemID).Result;
                model = PagesManagementFunctions.PageToPageModel(page);
            }
            else
                HttpContext.Items["IsIncorrect"] = true;
            if (model == null)
                return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.Categories}");
            return View("CategoriesAndProducts/EditProduct", model);
        }
    }
}