using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Treynessen.Functions;
using Treynessen.Extensions;
using Treynessen.Localization;
using Treynessen.LogManagement;
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
            ProductPage editableProduct = db.ProductPages.AsNoTracking().FirstOrDefault(pp => pp.ID == productID.Value);
            if (editableProduct == null)
            {
                successfullyCompleted = false;
                return;
            }
            model.ID = editableProduct.ID;
            model.PreviousPageID = editableProduct.PreviousPageID;
            ProductPage editedProduct = PagesManagementFunctions.PageModelToPage(db, model, context) as ProductPage;
            if (editedProduct == null)
            {
                successfullyCompleted = false;
                return;
            }
            editedProduct.PreviousPage.LastProductTemplate = editedProduct.Template;
            db.ProductPages.Update(editedProduct);
            db.SaveChanges();

            // Изменяем имена изображений продукта, если изменился псевдоним страницы
            if (!editableProduct.Alias.Equals(editedProduct.Alias, StringComparison.InvariantCulture))
            {
                IHostingEnvironment env = context.RequestServices.GetRequiredService<IHostingEnvironment>();
                string pathToImages = $"{env.GetProductsImagesFolderFullPath()}{editedProduct.PreviousPageID}{editedProduct.ID}\\";
                if (Directory.Exists(pathToImages))
                {
                    string oldName = editableProduct.Alias;
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
                    LinkedList<KeyValuePair<string, string>> renameErrors = new LinkedList<KeyValuePair<string, string>>();
                    string shortPathToImages = pathToImages.Replace(env.GetStorageFolderFullPath(), string.Empty).Replace('\\', '/').Insert(0, "/");
                    for (int i = 0; i < numOfImages; ++i)
                    {
                        string oldImageName = $"{oldName}{(i == 0 ? string.Empty : $"_{i.ToString()}")}";
                        string newImageName = $"{newName}{(i == 0 ? string.Empty : $"_{i.ToString()}")}";
                        // Изменяем данные в БД
                        Image image = db.Images.FirstOrDefault(img => img.ShortPathHash == OtherFunctions.GetHashFromString($"{shortPathToImages}{newImageName}.jpg")
                        && img.ShortPath.Equals($"{shortPathToImages}{newImageName}.jpg", StringComparison.InvariantCulture));
                        if (image != null)
                            db.Images.Remove(image);
                        image = db.Images.FirstOrDefault(img => img.ShortPathHash == OtherFunctions.GetHashFromString($"{shortPathToImages}{oldImageName}.jpg")
                        && img.ShortPath.Equals($"{shortPathToImages}{oldImageName}.jpg", StringComparison.InvariantCulture));
                        if (image != null)
                        {
                            image.ShortPath = image.ShortPath.Replace(oldImageName, newImageName);
                            image.ShortPathHash = OtherFunctions.GetHashFromString(image.ShortPath);
                            image.FullName = image.ShortPath.Substring(image.ShortPath.LastIndexOf('/') + 1);
                        }
                        try
                        {
                            ImagesManagementFunctions.RenameImageAndDependencies(
                                pathToImages: pathToImages,
                                oldImageName: oldImageName,
                                newImageName: newImageName,
                                imageExtension: ".jpg"
                            );
                        }
                        catch (IOException)
                        {
                            // Добавляем все ошибки переименования в список для второй попытки. Например, старое название было
                            // "Название" и мы переименовали страницу на "Название_2", но у товара было несколько картинок, 
                            // соответственно при ренейминге будет попытка присвоить первой картинке название Название_2, что приведет
                            // к ошибке, т.к. картинка с таким названием уже существует. Поэтому после первого прохода сделаем второй,
                            // что поможет избежать этих ошибок переименования
                            renameErrors.AddLast(new KeyValuePair<string, string>(oldImageName, newImageName));
                        }
                    }
                    db.SaveChanges();
                    if (renameErrors.Count > 0)
                    {
                        foreach (var e in renameErrors)
                        {
                            ImagesManagementFunctions.RenameImageAndDependencies(
                                pathToImages: pathToImages,
                                oldImageName: e.Key,
                                newImageName: e.Value,
                                imageExtension: ".jpg"
                            );
                        }
                    }
                }
            }
            successfullyCompleted = true;

            LogManagementFunctions.AddAdminPanelLog(
                db: db,
                context: context,
                info: $"{editableProduct.PageName} (ID-{editableProduct.ID.ToString()}): {(context.Items["LogLocalization"] as IAdminPanelLogLocalization)?.ProductEdited}"
            );
        }
    }
}