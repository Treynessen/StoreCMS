using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Treynessen.AdminPanelTypes;
using Treynessen.Database.Entities;
using Treynessen.ImagesManagement;

namespace Treynessen.Controllers
{
    public partial class AdminPanelController : Controller
    {
        [NonAction]
        public IActionResult ProductImages(int? itemID)
        {
            HttpContext.Items["pageID"] = AdminPanelPages.ProductImages;
            if (!itemID.HasValue)
                return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.Categories}");
            ProductPage product = db.ProductPages.AsNoTracking().FirstOrDefault(pp => pp.ID == itemID);
            if (product == null)
                return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.Categories}");
            IHostingEnvironment env = HttpContext.RequestServices.GetService<IHostingEnvironment>();
            string[] productImages = ImagesManagementFunctions.GetProductImageUrls(product, env);
            HttpContext.Items["ProductPage"] = product;
            return View("CategoriesAndProducts/ProductImages", productImages);
        }
    }
}