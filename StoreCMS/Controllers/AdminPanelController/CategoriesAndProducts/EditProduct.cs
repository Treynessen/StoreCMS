using System.Linq;
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
        public IActionResult EditProduct(int? itemID)
        {
            HttpContext.Items["pageID"] = AdminPanelPages.EditProduct;
            if (!itemID.HasValue)
                return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.Categories}");
            ProductPage page = db.ProductPages.AsNoTracking().FirstOrDefault(pp => pp.ID == itemID);
            if(page == null)
                return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.Categories}");
            PageModel model = PagesManagementFunctions.PageToPageModel(page);
            HttpContext.Items["Templates"] = db.Templates.AsNoTracking().ToArray();
            return View("CategoriesAndProducts/EditProduct", model);
        }
    }
}