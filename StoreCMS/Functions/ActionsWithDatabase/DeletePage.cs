using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Treynessen.AdminPanelTypes;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;

namespace Treynessen.Functions
{
    public static partial class ActionsWithDatabase
    {
        public static void DeletePage(CMSDatabase db, PageType? pageType, int? itemID)
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
            else
                db.Remove(page);
            db.SaveChanges();
        }

        public static void DeletePage(CMSDatabase db, Page page)
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
            else
                db.Remove(page);
            db.SaveChanges();
        }
    }
}