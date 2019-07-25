using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Treynessen.Functions;
using Treynessen.Extensions;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;

namespace Treynessen.ImagesManagement
{
    public static partial class ImagesManagementFunctions
    {
        public static void DeleteProductImage(CMSDatabase db, int? productID, int? imageID, HttpContext context, out bool successfullyDeleted)
        {
            if (!productID.HasValue || !imageID.HasValue)
            {
                successfullyDeleted = false;
                return;
            }
            ProductPage product = db.ProductPages.AsNoTracking().FirstOrDefault(p => p.ID == productID.Value);
            if (product == null)
            {
                successfullyDeleted = false;
                return;
            }
            IHostingEnvironment env = context.RequestServices.GetService<IHostingEnvironment>();
            string imagesPath = $"{env.GetProductsImagesFolderFullPath()}{product.PreviousPageID.ToString()}{product.ID.ToString()}\\";
            string imageFullName = $"{product.Alias}{(imageID == 0 ? string.Empty : $"_{imageID.Value}")}.jpg";
            DeleteImage(imagesPath, imageFullName, db, env);
            // Смещаем нумерацию изображений, идущих после удаленного на -1 и изменяем данные в БД
            Regex imagesChecker = new Regex($"{product.Alias}(_\\d+)?.jpg$");
            string[] images = Directory.GetFiles(imagesPath, $"*{product.Alias}*.jpg");
            int numOfImages = (from img in images
                               where imagesChecker.IsMatch(img)
                               select img).Count();
            string shortPathToImages = imagesPath.Replace(env.GetStorageFolderFullPath(), string.Empty).Replace('\\', '/').Insert(0, "/");
            for (int i = imageID.Value; i < numOfImages; ++i)
            {
                RenameImageAndDependencies(
                            pathToImages: imagesPath,
                            oldImageName: $"{product.Alias}_{(i + 1).ToString()}",
                            newImageName: $"{product.Alias}{(i == 0 ? string.Empty : $"_{i.ToString()}")}",
                            imageExtension: ".jpg"
                );
                string shortPathToImage = $"{shortPathToImages}{product.Alias}_{(i + 1).ToString()}.jpg";
                Image image = db.Images.FirstOrDefault(img => img.ShortPathHash == OtherFunctions.GetHashFromString(shortPathToImage) 
                && img.ShortPath.Equals(shortPathToImage, StringComparison.InvariantCulture));
                if (image != null)
                {
                    image.ShortPath = $"{shortPathToImages}{product.Alias}{(i == 0 ? string.Empty : $"_{i.ToString()}")}.jpg";
                    image.ShortPathHash = OtherFunctions.GetHashFromString(image.ShortPath);
                    image.FullName = image.ShortPath.Substring(image.ShortPath.LastIndexOf('/') + 1);
                }
            }
            db.SaveChanges();
            successfullyDeleted = true;
        }
    }
}