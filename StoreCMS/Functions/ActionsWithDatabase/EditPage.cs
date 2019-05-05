using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Treynessen.Extensions;
using Treynessen.AdminPanelTypes;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;

namespace Treynessen.Functions
{
    public static partial class ActionsWithDatabase
    {
        public static bool EditPage(CMSDatabase db, AdminPanelModel model, HttpContext context)
        {
            if (!model.itemID.HasValue || !model.PageType.HasValue || model.PageModel == null)
                return false;
            ProductPage changedProductPage = null;
            bool isMainPage = false;
            switch (model.PageType)
            {
                case PageType.Usual:
                    UsualPage usualPage = db.UsualPages.AsNoTracking().FirstOrDefaultAsync(up => up.ID == model.itemID).Result;
                    if (usualPage == null)
                        return false;
                    isMainPage = !usualPage.PreviousPageID.HasValue && usualPage.Alias.Equals("index", StringComparison.InvariantCultureIgnoreCase);
                    break;
                case PageType.Category:
                    CategoryPage categoryPage = db.CategoryPages.AsNoTracking().FirstOrDefaultAsync(cp => cp.ID == model.itemID).Result;
                    if (categoryPage == null)
                        return false;
                    break;
                case PageType.Product:
                    changedProductPage = db.ProductPages.AsNoTracking().FirstOrDefaultAsync(pp => pp.ID == model.itemID).Result;
                    if (changedProductPage == null)
                        return false;
                    model.PageModel.PreviousPageID = changedProductPage.PreviousPageID;
                    break;
            }
            model.PageModel.PageType = model.PageType.Value;
            Page page = OtherFunctions.PageModelToPage(db, model.PageModel, context, isMainPage);
            if (page != null)
            {
                page.ID = model.itemID.Value;
                if (page is UsualPage up)
                {
                    if (up.PreviousPageID.HasValue && up.PreviousPage.ID == up.ID)
                        return false;
                }
            }
            else return false;
            if (!Validator.TryValidateObject(page, new ValidationContext(page), null))
                return false;
            OtherFunctions.SetUniqueAliasName(db, page);

            if (page is ProductPage product)
            {
                if (!changedProductPage.PageName.Equals(product.PageName, StringComparison.InvariantCulture))
                {
                    IHostingEnvironment env = context.RequestServices.GetRequiredService<IHostingEnvironment>();
                    string pathToImages = $"{env.GetProductsImagesPath()}{changedProductPage.PreviousPageID}{changedProductPage.ID}\\";
                    if (Directory.Exists(pathToImages))
                    {
                        LinkedList<KeyValuePair<string, string>> listOfChanges = new LinkedList<KeyValuePair<string, string>>();
                        string oldName = OtherFunctions.GetCorrectName(changedProductPage.PageName, context);
                        string newName = OtherFunctions.GetCorrectName(product.PageName, context);
                        Regex imagesChecker = new Regex($"{oldName}(_\\d+)?.jpg$");
                        string[] oldImagesNames = Directory.GetFiles(pathToImages, $"*{oldName}*.jpg");
                        oldImagesNames = (from img in oldImagesNames
                                          where imagesChecker.IsMatch(img)
                                          select img).ToArray();
                        for (int i = 0; i < oldImagesNames.Length; ++i)
                        {
                            OtherFunctions.RenameImage(
                                pathToImages: pathToImages,
                                oldImageName: $"{oldName}{(i == 0 ? string.Empty : $"_{i}")}",
                                newImageName: $"{newName}{(i == 0 ? string.Empty : $"_{i}")}",
                                listOfChanges: listOfChanges
                            );
                        }
                        if (listOfChanges.Count > 0)
                        {
                            string imagesInfoPath = $"{pathToImages}images.info";
                            OtherFunctions.ReplaceContentInFile(imagesInfoPath, listOfChanges);
                        }
                    }
                }
            }

            db.Update(page);
            RefreshPageAndDependencies(db, page);
            db.SaveChanges();
            return true;
        }
    }
}
