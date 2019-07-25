using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Treynessen.Functions;
using Treynessen.Extensions;
using Treynessen.PagesManagement;
using Treynessen.ImagesManagement;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;

namespace Treynessen.Database
{
    public static partial class DatabaseInteraction
    {
        public static void DeletePage(CMSDatabase db, PageType? pageType, int? itemID, HttpContext context, out bool successfullyDeleted)
        {
            if (!pageType.HasValue || !itemID.HasValue)
            {
                successfullyDeleted = false;
                return;
            }
            Page page = null;
            switch (pageType)
            {
                case PageType.Usual:
                    page = db.UsualPages.FirstOrDefault(p => p.ID == itemID);
                    break;
                case PageType.Category:
                    page = db.CategoryPages.FirstOrDefault(p => p.ID == itemID);
                    break;
                default:
                    successfullyDeleted = false;
                    return;
            }
            if (page == null)
            {
                successfullyDeleted = false;
                return;
            }
            if (page is UsualPage up)
            {
                // Получаем все зависимые страницы
                List<UsualPage> usualPages = db.UsualPages.Where(p => p.PreviousPageID == up.ID).ToList();
                List<CategoryPage> categoryPages = db.CategoryPages.Where(p => p.PreviousPageID == up.ID).ToList();
                db.UsualPages.Remove(up);
                db.SaveChanges();
                // Обновляем полученные зависимые страницы
                foreach (var u_page in usualPages)
                {
                    u_page.PreviousPageID = up.PreviousPageID;
                    RefreshPageAndDependencies(db, u_page);
                }
                foreach (var c_page in categoryPages)
                {
                    c_page.PreviousPageID = up.PreviousPageID;
                    RefreshPageAndDependencies(db, c_page);
                }
            }
            else if (page is CategoryPage cp)
            {
                IHostingEnvironment env = context.RequestServices.GetRequiredService<IHostingEnvironment>();
                cp.ProductPages = db.ProductPages.Where(pp => pp.PreviousPageID == cp.ID).ToList();
                foreach (var p in cp.ProductPages)
                {
                    string[] images = ImagesManagementFunctions.GetProductImageUrls(p, env);
                    for (int i = 0; i < images.Length; ++i)
                    {
                        Image image = db.Images.FirstOrDefault(img => img.ShortPathHash == OtherFunctions.GetHashFromString(images[i])
                        && img.ShortPath.Equals(images[i], StringComparison.InvariantCulture));
                        if (image != null)
                            db.Images.Remove(image);
                    }
                    string pathToImages = $"{env.GetProductsImagesFolderFullPath()}{p.PreviousPageID}{p.ID}\\";
                    if (Directory.Exists(pathToImages))
                        Directory.Delete(pathToImages, true);
                }
                db.Remove(page);
            }
            db.SaveChanges();
            successfullyDeleted = true;
        }
    }
}