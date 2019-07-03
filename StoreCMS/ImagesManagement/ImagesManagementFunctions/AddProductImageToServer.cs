using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Treynessen.Extensions;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;
using Treynessen.FileManagerManagement;

namespace Treynessen.ImagesManagement
{
    public static partial class ImagesManagementFunctions
    {
        public static void AddProductImageToServer(CMSDatabase db, IFormFile file, int? itemID, HttpContext context)
        {
            if (!itemID.HasValue || file == null)
                return;
            ProductPage product = db.ProductPages.FirstOrDefaultAsync(pp => pp.ID == itemID).Result;
            db.Entry(product).State = EntityState.Detached;
            if (product == null)
                return;
            IHostingEnvironment env = context.RequestServices.GetRequiredService<IHostingEnvironment>();
            string imagesPath = $"{env.GetProductsImagesFolderFullPath()}{product.PreviousPageID.ToString()}{product.ID.ToString()}\\";
            Directory.CreateDirectory(imagesPath);
            string fullImageName = FileManagerManagementFunctions.GetUniqueFileOrFolderName(imagesPath, product.Alias, ".jpg");
            string pathToFile = $"{imagesPath}{fullImageName}";
            using (Stream stream = file.OpenReadStream())
            {
                try
                {
                    using (Image<Rgba32> source = Image.Load(stream))
                    {
                        // Если остались зависимости от предыдущего изображения, то удаляем их
                        DeleteDependentImages(imagesPath, fullImageName); 
                        AddImageInfoInInfoFile(imagesPath, fullImageName, source.Width, source.Height);
                        source.Save(pathToFile);
                    }
                }
                catch (System.NotSupportedException) { }
            }
        }
    }
}