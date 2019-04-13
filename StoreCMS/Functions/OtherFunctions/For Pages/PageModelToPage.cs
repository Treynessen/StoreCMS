using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Treynessen.AdminPanelTypes;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;

namespace Treynessen.Functions
{
    public static partial class OtherFunctions
    {
        public static Page PageModelToPage(CMSDatabase db, PageModel model, HttpContext context)
        {
            if (!model.PageType.HasValue)
                return null;

            Page page = null;
            switch (model.PageType.Value)
            {
                case PageType.Usual:
                    page = new UsualPage();
                    if (model.IsMainPage && !model.PreviousPageID.HasValue && !HasMainPage(db)) // ←
                        page.Alias = "index"; // ←
                    else if (model.IsMainPage) // ←
                        return null; // ←
                    if (model.PreviousPageID.HasValue)
                        (page as UsualPage).PreviousPage = db.UsualPages.FirstOrDefaultAsync(up => up.ID == model.PreviousPageID.Value).Result;
                    page.RequestPathWithoutAlias = (page as UsualPage).PreviousPage == null ? "/"
                        : $"{GetUrl((page as UsualPage).PreviousPage)}";
                    break;
                case PageType.Category:
                    if (model.IsMainPage) // ←
                        return null; // ←
                    page = new CategoryPage();
                    if (string.IsNullOrEmpty(model.CategoryName)) // ←
                        (page as CategoryPage).CategoryName = model.BreadcrumbName; // ←
                    else
                        (page as CategoryPage).CategoryName = model.CategoryName;
                    if (model.PreviousPageID.HasValue)
                        (page as CategoryPage).PreviousPage = db.UsualPages.FirstOrDefaultAsync(up => up.ID == model.PreviousPageID.Value).Result;
                    page.RequestPathWithoutAlias = (page as CategoryPage).PreviousPage == null ? "/"
                        : $"{GetUrl((page as CategoryPage).PreviousPage)}";
                    break;
                case PageType.Product:
                    if (model.IsMainPage) // ←
                        return null; // ←
                    page = new ProductPage();
                    (page as ProductPage).ProductName = model.ProductName;
                    (page as ProductPage).Price = model.Price;
                    (page as ProductPage).OldPrice = model.OldPrice;
                    (page as ProductPage).ShortDescription = model.ShortDescription;
                    if (model.PreviousPageID.HasValue)
                        (page as ProductPage).PreviousPage = db.CategoryPages.FirstOrDefaultAsync(cp => cp.ID == model.PreviousPageID.Value).Result;
                    page.RequestPathWithoutAlias = (page as ProductPage).PreviousPage == null ? "/"
                        : $"{GetUrl((page as ProductPage).PreviousPage)}";
                    break;
                default:
                    return null;
            }
            page.Title = model.Title;
            page.BreadcrumbName = model.BreadcrumbName;

            if (string.IsNullOrEmpty(page.Alias))
            {
                if (string.IsNullOrEmpty(model.Alias)) // ←
                    page.Alias = GetCorrectAliasName(model.BreadcrumbName, context); // ←
                else 
                    page.Alias = GetCorrectAliasName(model.Alias, context); // ←
            }
            if (page.RequestPathWithoutAlias.Equals("/") && !model.IsMainPage && !string.IsNullOrEmpty(page.Alias) && page.Alias.Equals("index"))
                page.Alias = "ind";

            page.Content = model.Content;
            page.TemplatePath = model.TemplatePath;
            page.Published = model.Published;
            page.PageDescription = model.PageDescription;
            page.PageKeywords = model.PageKeywords;
            page.IsRobotIndex = model.IsRobotIndex;
            return page;
        }
    }
}