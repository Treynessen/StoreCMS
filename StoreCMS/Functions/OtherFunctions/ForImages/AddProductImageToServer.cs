using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Treynessen.Extensions;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;

namespace Treynessen.Functions
{
    public static partial class OtherFunctions
    {
        public static void AddProductImageToServer(CMSDatabase db, IFormFile file, int? itemID, HttpContext context)
        {
            if (!itemID.HasValue)
                return;
            ProductPage productPage = db.ProductPages.FirstOrDefaultAsync(pp => pp.ID == itemID).Result;
            if (productPage == null)
                return;
            IHostingEnvironment env = context.RequestServices.GetRequiredService<IHostingEnvironment>();
            string imagesPath = $"{env.GetProductsInImagesPath()}{productPage.PreviousPageID}{productPage.ID}\\";
            Directory.CreateDirectory(imagesPath);
            string fileName = GetUniqueProductImageName(imagesPath, productPage.Alias);
            string pathToFile = $"{imagesPath}{fileName}";
            using (FileStream fs = new FileStream(pathToFile, FileMode.Create))
            {
                file.CopyTo(fs);
            }
        }
    }
}
