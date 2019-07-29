using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Treynessen.Functions;
using Treynessen.Extensions;
using Treynessen.Localization;
using Treynessen.LogManagement;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;
using Treynessen.FileManagerManagement;

namespace Treynessen.ImagesManagement
{
    public static partial class ImagesManagementFunctions
    {
        public static void UploadProductImageToServer(CMSDatabase db, IFormFile file, int? itemID, HttpContext context, out bool successfullyUploaded)
        {
            successfullyUploaded = true;
            if (!itemID.HasValue || file == null)
            {
                successfullyUploaded = false;
                return;
            }
            ProductPage product = db.ProductPages.AsNoTracking().FirstOrDefault(pp => pp.ID == itemID);
            if (product == null)
            {
                successfullyUploaded = false;
                return;
            }
            IHostingEnvironment env = context.RequestServices.GetRequiredService<IHostingEnvironment>();
            string imagesPath = $"{env.GetProductsImagesFolderFullPath()}{product.PreviousPageID.ToString()}{product.ID.ToString()}\\";
            Directory.CreateDirectory(imagesPath);
            string fullImageName = FileManagerManagementFunctions.GetUniqueFileOrFolderName(imagesPath, product.Alias, ".jpg");
            string pathToFile = $"{imagesPath}{fullImageName}";
            using (Stream stream = file.OpenReadStream())
            {
                try
                {
                    using (Image<Rgba32> source = SixLabors.ImageSharp.Image.Load(stream))
                    {
                        // Если остались зависимости от предыдущего изображения, то удаляем их
                        DeleteDependentImages(imagesPath, fullImageName);
                        // Добавляем или изменяем информацию в БД
                        string shortPathToImage = pathToFile.Replace(env.GetStorageFolderFullPath(), string.Empty).Replace('\\', '/').Insert(0, "/");
                        Database.Entities.Image image = db.Images.FirstOrDefault(img => img.ShortPathHash == OtherFunctions.GetHashFromString(shortPathToImage)
                        && img.ShortPath.Equals(shortPathToImage, StringComparison.InvariantCulture));
                        if (image == null)
                        {
                            image = new Database.Entities.Image
                            {
                                ShortPath = shortPathToImage,
                                ShortPathHash = OtherFunctions.GetHashFromString(shortPathToImage),
                                FullName = shortPathToImage.Substring(shortPathToImage.LastIndexOf('/') + 1),
                                Width = (uint)source.Width,
                                Height = (uint)source.Height
                            };
                            db.Images.Add(image);
                        }
                        else // Если вдруг каким-то образом информация об изображении не была удалена из БД
                        {
                            image.Width = (uint)source.Width;
                            image.Height = (uint)source.Height;
                        }
                        db.SaveChanges();
                        source.Save(pathToFile);
                        LogManagementFunctions.AddAdminPanelLog(
                            db: db,
                            context: context,
                            info: $"{product.PageName} (ID-{product.ID.ToString()}): {(context.Items["LogLocalization"] as IAdminPanelLogLocalization)?.ProductImageUploaded}"
                        );
                    }
                }
                catch (NotSupportedException) { successfullyUploaded = false; }
            }
        }
    }
}