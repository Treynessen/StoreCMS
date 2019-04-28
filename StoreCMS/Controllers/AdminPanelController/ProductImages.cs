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
            ProductPage productPage = db.ProductPages.FirstOrDefaultAsync(pp => pp.ID == itemID).Result;
            if (productPage == null)
                return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.Categories}");
            IHostingEnvironment env = HttpContext.RequestServices.GetService<IHostingEnvironment>();
            string imagesPath = $"{env.GetProductsInImagesPath()}{productPage.PreviousPageID}{productPage.ID}\\";
            string shortImagesPath = $"{env.GetProductsInImagesPath(true)}{productPage.PreviousPageID}{productPage.ID}/";
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
                Regex regex = new Regex($"^{imagesPath.Replace(@"\", @"\\")}{productPage.Alias}(_(\\d)+)?.jpg$");
                productImages = (from image in productImages
                                 where regex.IsMatch(image)
                                 select image).ToArray();
            }
            return View("Products/ProductImages", productImages);
        }
    }
}
