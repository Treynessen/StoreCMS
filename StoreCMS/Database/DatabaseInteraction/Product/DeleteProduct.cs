using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Treynessen.Functions;
using Treynessen.Extensions;
using Treynessen.Localization;
using Treynessen.LogManagement;
using Treynessen.ImagesManagement;
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
            ProductPage product = db.ProductPages.FirstOrDefault(pp => pp.ID == productID.Value);
            if (product == null)
            {
                successfullyDeleted = false;
                return;
            }
            IHostingEnvironment env = context.RequestServices.GetRequiredService<IHostingEnvironment>();
            string pathToImages = $"{env.GetProductsImagesFolderFullPath()}{product.ID}/";
            // Удаляем данные об изображениях из БД
            string[] images = ImagesManagementFunctions.GetProductImageUrls(product, env);
            for (int i = 0; i < images.Length; ++i)
            {
                Image image = db.Images.FirstOrDefault(img => img.ShortPathHash == OtherFunctions.GetHashFromString(images[i])
                && img.ShortPath.Equals(images[i], StringComparison.Ordinal));
                if (image != null)
                    db.Images.Remove(image);
            }
            db.Entry(product).Reference(pp => pp.PreviousPage).Load();
            --product.PreviousPage.ProductsCount;
            db.ProductPages.Remove(product);
            db.SaveChanges();
            // Удаляем папку с изображениями товара
            if (Directory.Exists(pathToImages))
                Directory.Delete(pathToImages, true);
            successfullyDeleted = true;

            LogManagementFunctions.AddAdminPanelLog(
                db: db,
                context: context,
                info: $"{product.PageName} (ID-{product.ID.ToString()}): {(context.Items["LogLocalization"] as IAdminPanelLogLocalization)?.ProductDeleted}"
            );
        }
    }
}