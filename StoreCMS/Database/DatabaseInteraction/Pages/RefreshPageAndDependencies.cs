using System;
using System.Linq;
using System.Collections.Generic;
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
                    db.Entry(up).Reference(p => p.PreviousPage).Load();
                    string temp = up.RequestPathWithoutAlias;
                    up.RequestPathWithoutAlias = up.PreviousPage == null ? "/" : PagesManagementFunctions.GetUrl(up.PreviousPage);
                    up.BreadcrumbsHtml = PagesManagementFunctions.GetBreadcrumbsHTML(up);
                    if (!temp.Equals("/") && up.RequestPathWithoutAlias.Equals("/") && up.Alias.Equals("index", StringComparison.InvariantCulture))
                        up.Alias = "ind";
                    break;
                case CategoryPage cp:
                    db.Entry(cp).Reference(p => p.PreviousPage).Load();
                    cp.RequestPathWithoutAlias = cp.PreviousPage == null ? "/" : PagesManagementFunctions.GetUrl(cp.PreviousPage);
                    cp.BreadcrumbsHtml = PagesManagementFunctions.GetBreadcrumbsHTML(cp);
                    if (cp.RequestPathWithoutAlias.Equals("/") && cp.Alias.Equals("index", StringComparison.InvariantCulture))
                        cp.Alias = "ind";
                    break;
                case ProductPage pp:
                    db.Entry(pp).Reference(p => p.PreviousPage).Load();
                    pp.RequestPathWithoutAlias = PagesManagementFunctions.GetUrl(pp.PreviousPage);
                    pp.BreadcrumbsHtml = PagesManagementFunctions.GetBreadcrumbsHTML(pp);
                    break;
            }
            PagesManagementFunctions.SetUniqueAliasName(db, page);
            db.Update(page);
            switch (page)
            {
                case UsualPage up:
                    List<UsualPage> usualPages = db.UsualPages.Where(p => p.PreviousPageID == up.ID).ToList();
                    List<CategoryPage> categoryPages = db.CategoryPages.Where(p => p.PreviousPageID == up.ID).ToList();
                    foreach (var u_page in usualPages)
                        RefreshPageAndDependencies(db, u_page);
                    foreach (var c_page in categoryPages)
                        RefreshPageAndDependencies(db, c_page);
                    break;
                case CategoryPage cp:
                    List<ProductPage> productPages = db.ProductPages.Where(p => p.PreviousPageID == cp.ID).ToList();
                    foreach (var p_page in productPages)
                        RefreshPageAndDependencies(db, p_page);
                    break;
            }
        }
    }
}