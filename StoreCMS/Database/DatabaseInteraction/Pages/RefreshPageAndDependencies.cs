using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Treynessen.PagesManagement;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;

namespace Treynessen.Database
{
    public static partial class DatabaseInteraction
    {
        // ЭТОТ МЕТОД НЕ СОХРАНЯЕТ ИЗМЕНЕНИЯ, ТОЛЬКО ВНОСИТ ИХ
        // После изменения или удаления страницы-родителя, необходимо изменить все страницы, наследующиеся от
        // этой родительской страницы.
        public static void RefreshPageAndDependencies(CMSDatabase db, Page page)
        {
            switch (page)
            {
                case UsualPage up:
                    db.Entry(up).Reference(p => p.PreviousPage).LoadAsync().Wait();
                    if (up.PreviousPage == null && up.Alias.Equals("index", StringComparison.InvariantCulture))
                        up.Alias = "ind";
                    if (up.PreviousPage == null || up.PreviousPage.RequestPath.Equals("/", StringComparison.InvariantCulture))
                        up.RequestPath = $"/{up.Alias}";
                    else up.RequestPath = $"{up.PreviousPage.RequestPath}/{up.Alias}";
                    up.RequestPath = up.PreviousPage == null ? $"/{up.Alias}" : $"{up.PreviousPage.RequestPath}/{up.Alias}";
                    up.BreadcrumbsHtml = PagesManagementFunctions.GetBreadcrumbsHTML(up);
                    break;
                case CategoryPage cp:
                    db.Entry(cp).Reference(p => p.PreviousPage).LoadAsync().Wait();
                    if (cp.PreviousPage == null && cp.Alias.Equals("index", StringComparison.InvariantCulture))
                        cp.Alias = "ind";
                    if (cp.PreviousPage == null || cp.PreviousPage.RequestPath.Equals("/", StringComparison.InvariantCulture))
                        cp.RequestPath = $"/{cp.Alias}";
                    else cp.RequestPath = $"{cp.PreviousPage.RequestPath}/{cp.Alias}";
                    cp.BreadcrumbsHtml = PagesManagementFunctions.GetBreadcrumbsHTML(cp);
                    break;
                case ProductPage pp:
                    db.Entry(pp).Reference(p => p.PreviousPage).LoadAsync().Wait();
                    pp.RequestPath = $"{pp.PreviousPage.RequestPath}/{pp.Alias}";
                    pp.BreadcrumbsHtml = PagesManagementFunctions.GetBreadcrumbsHTML(pp);
                    break;
            }
            PagesManagementFunctions.SetUniqueAliasName(db, page);
            db.Update(page);
            switch (page)
            {
                case UsualPage up:
                    List<UsualPage> usualPages = db.UsualPages.Where(p => p.PreviousPageID == up.ID).ToListAsync().Result;
                    List<CategoryPage> categoryPages = db.CategoryPages.Where(p => p.PreviousPageID == up.ID).ToListAsync().Result;
                    foreach (var u_page in usualPages)
                        RefreshPageAndDependencies(db, u_page);
                    foreach (var c_page in categoryPages)
                        RefreshPageAndDependencies(db, c_page);
                    break;
                case CategoryPage cp:
                    List<ProductPage> productPages = db.ProductPages.Where(p => p.PreviousPageID == cp.ID).ToListAsync().Result;
                    foreach (var p_page in productPages)
                        RefreshPageAndDependencies(db, p_page);
                    break;
            }
        }
    }
}