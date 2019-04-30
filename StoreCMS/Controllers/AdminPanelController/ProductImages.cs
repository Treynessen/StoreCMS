using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Treynessen.Extensions;
using Treynessen.AdminPanelTypes;
using Treynessen.Database.Entities;

namespace Treynessen.Controllers
{
    public partial class AdminPanelController : Controller
    {
        [NonAction]
        public IActionResult ProductImages(int? itemID)
        {
            if (!itemID.HasValue)
                return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.Categories}");
            ProductPage product = db.ProductPages.FirstOrDefaultAsync(pp => pp.ID == itemID).Result;
            if (product == null)
                return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.Categories}");
            db.Entry(product).State = EntityState.Detached;
            IHostingEnvironment env = HttpContext.RequestServices.GetService<IHostingEnvironment>();
            string imagesPath = $"{env.GetProductsImagesPath()}{product.PreviousPageID}{product.ID}\\";
            string shortImagesPath = $"{env.GetProductsImagesPath(true)}{product.PreviousPageID}{product.ID}/";
            string[] productImages = null;
            try
            {
                productImages = Directory.GetFiles(imagesPath, "*.jpg");
            }
            catch (DirectoryNotFoundException)
            {
                productImages = new string[0];
            }
            if (productImages.Length > 0)
            {
                Regex regex = new Regex($"{product.Alias}(_(\\d)+)?.jpg$");
                productImages = (from img in productImages
                                 where regex.IsMatch(img)
                                 let imageNameEnding = regex.Match(img).Value.Substring(product.Alias.Length)
                                 orderby imageNameEnding.Length == 4 ? 0 : Convert.ToInt32(imageNameEnding.Substring(1, imageNameEnding.IndexOf('.') - 1))
                                 select img).ToArray();
            }
            return View("Products/ProductImages", productImages);
        }
    }
}
