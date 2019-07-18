using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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
            ProductPage product = db.ProductPages.FirstOrDefaultAsync(p => p.ID == productID.Value).Result;
            if (product == null)
            {
                successfullyDeleted = false;
                return;
            }
            db.Entry(product).State = EntityState.Detached;
            IHostingEnvironment env = context.RequestServices.GetService<IHostingEnvironment>();
            string imagesPath = $"{env.GetProductsImagesFolderFullPath()}{product.PreviousPageID.ToString()}{product.ID.ToString()}\\";
            string imageFullName = $"{product.Alias}{(imageID == 0 ? string.Empty : $"_{imageID.Value}")}.jpg";
            DeleteImage(imagesPath, imageFullName, context);
            // Смещаем нумерацию изображений, идущих после удаленного на -1
            Regex imagesChecker = new Regex($"{product.Alias}(_\\d+)?.jpg$");
            string[] images = Directory.GetFiles(imagesPath, $"*{product.Alias}*.jpg");
            int numOfImages = (from img in images
                               where imagesChecker.IsMatch(img)
                               select img).Count();
            for (int i = imageID.Value; i < numOfImages; ++i)
            {
                RenameImageAndDependencies(
                            pathToImages: imagesPath,
                            oldImageName: $"{product.Alias}_{(i + 1).ToString()}",
                            newImageName: $"{product.Alias}{(i == 0 ? string.Empty : $"_{i.ToString()}")}"
                );
            }
            successfullyDeleted = true;
        }
    }
}