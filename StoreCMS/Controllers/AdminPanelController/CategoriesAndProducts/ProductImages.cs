using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Treynessen.Functions;
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
            if (Directory.Exists(imagesPath))
            {
                string imageNameBasis = product.Alias;
                productImages = Directory.GetFiles(imagesPath, $"*{imageNameBasis}*.jpg");
                if (productImages.Length > 0)
                {
                    Regex regex = new Regex($"{imageNameBasis}(_(\\d)+)?.jpg$");
                    productImages = (from img in productImages
                                     where regex.IsMatch(img)
                                     let imageNameEnding = regex.Match(img).Value.Substring(imageNameBasis.Length)
                                     orderby imageNameEnding.Length == 4 ? 0 : Convert.ToInt32(imageNameEnding.Substring(1, imageNameEnding.IndexOf('.') - 1))
                                     select img).ToArray();
                }
            }
            if (productImages == null)
                productImages = new string[0];
            return View("Products/ProductImages", productImages);
        }
    }
}
