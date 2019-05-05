using System;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Treynessen.AdminPanelTypes;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;

namespace Treynessen.Functions
{
    public static partial class OtherFunctions
    {
        public static Page PageModelToPage(CMSDatabase db, PageModel model, HttpContext context, bool isMainPage = false)
        {
            if (model == null)
                return null;
            if (!model.PageType.HasValue)
                return null;

            Page page = null;
            switch (model.PageType.Value)
            {
                case PageType.Usual:
                    UsualPage usualPage = new UsualPage();
                    page = usualPage;
                    if (isMainPage || (model.IsMainPage && !model.PreviousPageID.HasValue && !HasMainPage(db))) // ←
                        page.Alias = "index"; // ←
                    else if (model.IsMainPage) // ←
                        return null; // ←
                    if (model.PreviousPageID.HasValue)
                        usualPage.PreviousPage = db.UsualPages.FirstOrDefaultAsync(up => up.ID == model.PreviousPageID.Value).Result;
                    page.RequestPathWithoutAlias = usualPage.PreviousPage == null ? "/"
                        : GetUrl(usualPage.PreviousPage);
                    break;
                case PageType.Category:
                    if (model.IsMainPage) // ←
                        return null; // ←
                    CategoryPage categoryPage = new CategoryPage();
                    page = categoryPage;
                    if (model.PreviousPageID.HasValue)
                        categoryPage.PreviousPage = db.UsualPages.FirstOrDefaultAsync(up => up.ID == model.PreviousPageID.Value).Result;
                    page.RequestPathWithoutAlias = categoryPage.PreviousPage == null ? "/"
                        : GetUrl(categoryPage.PreviousPage);
                    break;
                case PageType.Product:
                    if (model.IsMainPage) // ←
                        return null; // ←
                    if (!model.PreviousPageID.HasValue) // ←
                        return null; // ←
                    CategoryPage productCategory = null; // ←
                    productCategory = db.CategoryPages.FirstOrDefaultAsync(cp => cp.ID == model.PreviousPageID).Result; // ←
                    if (productCategory == null) // ←
                        return null; // ←
                    ProductPage productPage = new ProductPage();
                    page = productPage;
                    productPage.Price = model.Price;
                    productPage.OldPrice = model.OldPrice;
                    productPage.ShortDescription = model.ShortDescription;
                    productPage.LastUpdate = DateTime.Now;
                    productPage.PreviousPage = productCategory;
                    productPage.RequestPathWithoutAlias = GetUrl(productPage.PreviousPage);
                    break;
                default:
                    return null;
            }
            page.Title = model.Title;
            page.BreadcrumbName = model.BreadcrumbName;

            if (string.IsNullOrEmpty(page.Alias))
            {
                if (string.IsNullOrEmpty(model.Alias)) // ←
                    page.Alias = GetCorrectName(model.BreadcrumbName, context); // ←
                else 
                    page.Alias = GetCorrectName(model.Alias, context); // ←
            }
            if (page.RequestPathWithoutAlias.Equals("/") && !model.IsMainPage && !string.IsNullOrEmpty(page.Alias) && page.Alias.Equals("index", StringComparison.InvariantCulture))
                page.Alias = "ind";

            page.Content = model.Content;
            if (model.TemplateId.HasValue)
                page.Template = db.Templates.FirstOrDefaultAsync(t => t.ID == model.TemplateId).Result;
            page.Published = model.Published;
            page.PageDescription = model.PageDescription;
            page.PageKeywords = model.PageKeywords;
            page.IsRobotIndex = model.IsRobotIndex;
            return page;
        }
    }
}