using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Treynessen.Extensions;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;

namespace Treynessen.Database
{
    public static partial class DatabaseInteraction
    {
        public static void DeleteProduct(CMSDatabase db, int? productID, HttpContext context, out bool successfullyDeleted)
        {
            if (!productID.HasValue)
            {
                successfullyDeleted = false;
                return;
            }
            ProductPage deletedProduct = db.ProductPages.FirstOrDefault(pp => pp.ID == productID.Value);
            if (deletedProduct == null)
            {
                successfullyDeleted = false;
                return;
            }
            IHostingEnvironment env = context.RequestServices.GetRequiredService<IHostingEnvironment>();
            string pathToImages = $"{env.GetProductsImagesFolderFullPath()}{deletedProduct.PreviousPageID}{deletedProduct.ID}\\";
            if (Directory.Exists(pathToImages))
                Directory.Delete(pathToImages, true);
            db.Entry(deletedProduct).Reference(pp => pp.PreviousPage).Load();
            --deletedProduct.PreviousPage.ProductsCount;
            db.ProductPages.Remove(deletedProduct);
            db.SaveChanges();
            successfullyDeleted = true;
        }
    }
}