using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text;
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
            switch (model.PageType)
            {
                case PageType.Usual:
                    UsualPage usualPage = db.UsualPages.FirstOrDefaultAsync(up => up.ID == model.itemID).Result;
                    if (usualPage == null)
                        return false;
                    db.Entry(usualPage).State = EntityState.Detached;
                    break;
                case PageType.Category:
                    CategoryPage categoryPage = db.CategoryPages.FirstOrDefaultAsync(cp => cp.ID == model.itemID).Result;
                    if (categoryPage == null)
                        return false;
                    db.Entry(categoryPage).State = EntityState.Detached;
                    break;
                case PageType.Product:
                    ProductPage productPage = db.ProductPages.FirstOrDefaultAsync(pp => pp.ID == model.itemID).Result;
                    if (productPage == null)
                        return false;
                    model.PageModel.PreviousPageID = productPage.PreviousPageID;
                    db.Entry(productPage).State = EntityState.Detached;
                    break;
            }
            model.PageModel.PageType = model.PageType.Value;
            Page page = OtherFunctions.PageModelToPage(db, model.PageModel, context);
            if (page != null)
            {
                page.ID = model.itemID.Value;
                if (page is UsualPage up)
                {
                    if (up.PreviousPageID.HasValue && up.PreviousPage.ID == up.ID)
                        return false;
                }
            }
            if (!Validator.TryValidateObject(page, new ValidationContext(page), null))
                return false;
            OtherFunctions.SetUniqueAliasName(db, page);

            if (page is ProductPage product)
            {
                var changedProductTask = db.ProductPages.FirstOrDefaultAsync(p => p.ID == product.ID);
                IHostingEnvironment env = context.RequestServices.GetRequiredService<IHostingEnvironment>();
                string productsImagesPath = env.GetProductsImagesPath();
                string pathToImages = $"{productsImagesPath}{changedProductTask.Result.PreviousPageID}{changedProductTask.Result.ID}\\";
                db.Entry(changedProductTask.Result).State = EntityState.Detached;
                if (!changedProductTask.Result.BreadcrumbName.Equals(product.BreadcrumbName))
                {
                    if (Directory.Exists(pathToImages))
                    {
                        LinkedList<KeyValuePair<string, string>> listOfChanges = new LinkedList<KeyValuePair<string, string>>();
                        string oldName = OtherFunctions.GetCorrectName(changedProductTask.Result.BreadcrumbName, context);
                        string newName = OtherFunctions.GetCorrectName(product.BreadcrumbName, context);
                        Regex imagesChecker = new Regex($"{oldName}(_\\d+)?.jpg$");
                        string[] oldImagesNames = Directory.GetFiles(pathToImages, $"*{oldName}*.jpg");
                        oldImagesNames = (from img in oldImagesNames
                                          where imagesChecker.IsMatch(img)
                                          select img).ToArray();
                        for (int i = 0; i < oldImagesNames.Length; ++i)
                        {
                            OtherFunctions.RenameImage(pathToImages,
                                $"{oldName}{(i == 0 ? string.Empty : $"_{i}")}",
                                $"{newName}{(i == 0 ? string.Empty : $"_{i}")}",
                                listOfChanges);
                        }
                        if (listOfChanges.Count > 0)
                        {
                            string imagesInfoPath = $"{pathToImages}images.info";
                            StringBuilder builder = null;
                            using (StreamReader reader = new StreamReader(imagesInfoPath))
                            {
                                builder = new StringBuilder(reader.ReadToEnd());
                            }
                            using (StreamWriter writer = new StreamWriter(imagesInfoPath))
                            {
                                foreach (var c in listOfChanges)
                                {
                                    builder.Replace(c.Key, c.Value);
                                }
                                writer.Write(builder.ToString());
                            }
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
