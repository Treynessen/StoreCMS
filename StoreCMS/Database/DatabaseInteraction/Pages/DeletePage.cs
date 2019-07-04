using System.IO;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Treynessen.Extensions;
using Treynessen.PagesManagement;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;

namespace Treynessen.Database
{
    public static partial class DatabaseInteraction
    {
        public static void DeletePage(CMSDatabase db, PageType? pageType, int? itemID, HttpContext context)
        {
            if (!pageType.HasValue || !itemID.HasValue)
                return;
            Page page = null;
            switch (pageType)
            {
                case PageType.Usual:
                    page = db.UsualPages.FirstOrDefaultAsync(p => p.ID == itemID).Result;
                    break;
                case PageType.Category:
                    page = db.CategoryPages.FirstOrDefaultAsync(p => p.ID == itemID).Result;
                    break;
                default:
                    return;
            }
            if (page == null)
                return;
            if (page is UsualPage up)
            {
                db.Entry(up).Reference(p => p.PreviousPage).Load();
                // Получаем все зависимые страницы
                List<UsualPage> usualPages = db.UsualPages.Where(p => p.PreviousPageID == up.ID).ToList();
                List<CategoryPage> categoryPages = db.CategoryPages.Where(p => p.PreviousPageID == up.ID).ToList();
                db.UsualPages.Remove(up);
                db.SaveChanges();
                // Обновляем полученные зависимые страницы
                foreach (var u_page in usualPages)
                {
                    u_page.PreviousPage = up.PreviousPage;
                    RefreshPageAndDependencies(db, u_page);
                }
                foreach (var c_page in categoryPages)
                {
                    c_page.PreviousPage = up.PreviousPage;
                    RefreshPageAndDependencies(db, c_page);
                }
            }
            else if (page is CategoryPage cp)
            {
                IHostingEnvironment env = context.RequestServices.GetRequiredService<IHostingEnvironment>();
                db.Entry(cp).Collection(p => p.ProductPages).Load();
                foreach(var p in cp.ProductPages)
                {
                    string pathToImages = $"{env.GetProductsImagesFolderFullPath()}{p.PreviousPageID}{p.ID}\\";
                    if (Directory.Exists(pathToImages))
                        Directory.Delete(pathToImages, true);
                }
                db.Remove(page);
            }
            db.SaveChanges();
        }
    }
}