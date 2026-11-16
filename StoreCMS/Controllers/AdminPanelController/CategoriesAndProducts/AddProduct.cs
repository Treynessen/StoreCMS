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
        public IActionResult AddProduct(int? itemID)
        {
            HttpContext.Items["pageID"] = AdminPanelPages.AddProduct;
            if (!itemID.HasValue)
                return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.Categories}");
            var categoryPage = db.CategoryPages.AsNoTracking().FirstOrDefault(cp => cp.ID == itemID.Value);
            if (categoryPage == null)
                return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.Categories}");
            Template[] templates = db.Templates.AsNoTracking().ToArray();
            int? lastProductTemplateID = categoryPage.LastProductTemplateID;
            if (lastProductTemplateID.HasValue)
            {
                lastProductTemplateID = templates.FirstOrDefault(t => t.ID == lastProductTemplateID.Value) != null ? lastProductTemplateID : null;
            }
            HttpContext.Items["Templates"] = templates;
            HttpContext.Items["LastTemplate"] = lastProductTemplateID;
            HttpContext.Items["CategoryPage"] = categoryPage;
            return View("CategoriesAndProducts/AddProduct");
        }
    }
}