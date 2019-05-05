using System.IO;
using System.Linq;
using System.Collections.Generic;
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
        public static void DeletePage(CMSDatabase db, PageType? pageType, int? itemID, HttpContext context)
        {
            if (!pageType.HasValue)
                return;
            if (!itemID.HasValue)
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
                case PageType.Product:
                    page = db.ProductPages.FirstOrDefaultAsync(p => p.ID == itemID).Result;
                    break;
                default:
                    return;
            }
            if (page == null)
                return;
            if (page is UsualPage up)
            {
                db.Entry(up).Reference(p => p.PreviousPage).Load();
                List<UsualPage> usualPages = db.UsualPages.Where(p => p.PreviousPageID == up.ID).ToList();
                List<CategoryPage> categoryPages = db.CategoryPages.Where(p => p.PreviousPageID == up.ID).ToList();
                db.Remove(page);
                db.SaveChanges();
                foreach (var u_page in usualPages)
                {
                    u_page.PreviousPage = up.PreviousPage;
                    db.Update(u_page);
                    RefreshPageAndDependencies(db, u_page);
                }
                foreach (var c_page in categoryPages)
                {
                    c_page.PreviousPage = up.PreviousPage;
                    db.Update(c_page);
                    RefreshPageAndDependencies(db, c_page);
                }
            }
            else if (page is CategoryPage cp)
            {
                db.Entry(cp).Collection(p => p.ProductPages).Load();
                IHostingEnvironment env = context.RequestServices.GetRequiredService<IHostingEnvironment>();
                string productsImagesPath = env.GetProductsImagesPath();
                foreach (var pp in cp.ProductPages)
                {
                    string pathToImages = $"{productsImagesPath}{cp.ID}{pp.ID}\\";
                    if (Directory.Exists(pathToImages))
                        Directory.Delete(pathToImages, true);
                }
                db.Remove(page);
            }
            else if (page is ProductPage pp)
            {
                IHostingEnvironment env = context.RequestServices.GetRequiredService<IHostingEnvironment>();
                string pathToImages = $"{env.GetProductsImagesPath()}{pp.PreviousPageID}{pp.ID}\\";
                if (Directory.Exists(pathToImages))
                    Directory.Delete(pathToImages, true);
                db.Remove(page);
            }
            db.SaveChanges();
        }

        public static void DeletePage(CMSDatabase db, Page page, HttpContext context)
        {
            if (page == null)
                return;
            if (page is UsualPage up)
            {
                db.Entry(up).Reference(p => p.PreviousPage).Load();
                List<UsualPage> usualPages = db.UsualPages.Where(p => p.PreviousPageID == up.ID).ToList();
                List<CategoryPage> categoryPages = db.CategoryPages.Where(p => p.PreviousPageID == up.ID).ToList();
                db.Remove(page);
                db.SaveChanges();
                foreach (var u_page in usualPages)
                {
                    u_page.PreviousPage = up.PreviousPage;
                    db.Update(u_page);
                    RefreshPageAndDependencies(db, u_page);
                }
                foreach (var c_page in categoryPages)
                {
                    c_page.PreviousPage = up.PreviousPage;
                    db.Update(c_page);
                    RefreshPageAndDependencies(db, c_page);
                }
            }
            else if (page is CategoryPage cp)
            {
                db.Entry(cp).Collection(p => p.ProductPages).Load();
                IHostingEnvironment env = context.RequestServices.GetRequiredService<IHostingEnvironment>();
                string productsImagesPath = env.GetProductsImagesPath();
                foreach (var pp in cp.ProductPages)
                {
                    string pathToImages = $"{productsImagesPath}{cp.ID}{pp.ID}\\";
                    if (Directory.Exists(pathToImages))
                        Directory.Delete(pathToImages, true);
                }
                db.Remove(page);
            }
            else if (page is ProductPage pp)
            {
                IHostingEnvironment env = context.RequestServices.GetRequiredService<IHostingEnvironment>();
                string pathToImages = $"{env.GetProductsImagesPath()}{pp.PreviousPageID}{pp.ID}\\";
                if (Directory.Exists(pathToImages))
                    Directory.Delete(pathToImages, true);
                db.Remove(page);
            }
            db.SaveChanges();
        }
    }
}