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

namespace Treynessen.Functions
{
    public static partial class OtherFunctions
    {
        public static void AddProductImageToServer(CMSDatabase db, IFormFile file, int? itemID, HttpContext context)
        {
            if (!itemID.HasValue)
                return;
            if (file == null)
                return;
            ProductPage productPage = db.ProductPages.FirstOrDefaultAsync(pp => pp.ID == itemID).Result;
            if (productPage == null)
                return;
            IHostingEnvironment env = context.RequestServices.GetRequiredService<IHostingEnvironment>();
            string imagesPath = $"{env.GetProductsImagesPath()}{productPage.PreviousPageID}{productPage.ID}\\";
            Directory.CreateDirectory(imagesPath);
            string fileName = GetUniqueProductImageName(imagesPath, GetCorrectName(productPage.BreadcrumbName, context));
            string pathToFile = $"{imagesPath}{fileName}";
            using (Stream stream = file.OpenReadStream())
            {
                try
                {
                    using (Image<Rgba32> source = Image.Load(stream))
                    {
                        source.Save(pathToFile);
                    }
                }
                catch (System.NotSupportedException) { }
            }
        }
    }
}
