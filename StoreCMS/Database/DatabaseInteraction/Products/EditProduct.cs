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
using Treynessen.AdminPanelTypes;
using Treynessen.PagesManagement;
using Treynessen.ImagesManagement;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;

namespace Treynessen.Database
{
    public static partial class DatabaseInteraction
    {
        public static void EditProduct(CMSDatabase db, PageModel model, int? productID, HttpContext context, out bool successfullyCompleted)
        {
            if (model == null || !productID.HasValue)
            {
                successfullyCompleted = false;
                return;
            }
            model.PageType = PageType.Product;
            ProductPage changeableProduct = db.ProductPages.FirstOrDefaultAsync(pp => pp.ID == productID.Value).Result;
            if (changeableProduct == null)
            {
                successfullyCompleted = false;
                return;
            }
            db.Entry(changeableProduct).State = EntityState.Detached;
            model.PreviousPageID = changeableProduct.PreviousPageID;
            ProductPage editedProduct = PagesManagementFunctions.PageModelToPage(db, model, context) as ProductPage;
            if (editedProduct == null)
            {
                successfullyCompleted = false;
                return;
            }
            editedProduct.ID = changeableProduct.ID;
            PagesManagementFunctions.SetUniqueAliasName(db, editedProduct);
            db.ProductPages.Update(editedProduct);
            db.SaveChanges();

            // Изменяем имена изображений продукта, если изменился псевдоним страницы
            if (!changeableProduct.Alias.Equals(editedProduct.Alias, StringComparison.InvariantCulture))
            {
                IHostingEnvironment env = context.RequestServices.GetRequiredService<IHostingEnvironment>();
                string pathToImages = $"{env.GetProductsImagesFolderFullPath()}{editedProduct.PreviousPageID}{editedProduct.ID}\\";
                if (Directory.Exists(pathToImages))
                {
                    string oldName = changeableProduct.Alias;
                    string newName = editedProduct.Alias;
                    Regex imagesChecker = new Regex($"{oldName}(_\\d+)?.jpg$");
                    string[] oldImagesNames = Directory.GetFiles(pathToImages, $"*{oldName}*.jpg");
                    int numOfImages = (from img in oldImagesNames
                                      where imagesChecker.IsMatch(img)
                                      select img).Count();
                    // Можно было бы заменить разом имена всем изображениям через перебор в цикле, но
                    // проблема в том, что путь до папки с изображениями может содержать старое название
                    // изображения. В итоге замена имени через File.Move(старый_путь, старый_путь.Replace(oldName, newName))
                    // может привести к переносу изображений в другую директорию.
                    for (int i = 0; i < numOfImages; ++i)
                    {
                        ImagesManagementFunctions.RenameImageAndDependencies(
                            pathToImages: pathToImages,
                            oldImageName: $"{oldName}{(i == 0 ? string.Empty : $"_{i.ToString()}")}",
                            newImageName: $"{newName}{(i == 0 ? string.Empty : $"_{i.ToString()}")}",
                            editImagesInfoFile: false
                        );
                    }
                    OtherFunctions.ReplaceContentInFile($"{pathToImages}images.info", oldName, newName);
                }
            }
            successfullyCompleted = true;
        }
    }
}