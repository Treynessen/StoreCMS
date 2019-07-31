using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Treynessen.Functions;
using Treynessen.Extensions;
using Treynessen.AdminPanelTypes;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;

namespace Treynessen.PagesManagement
{
    public static partial class PagesManagementFunctions
    {
        public static Page PageModelToPage(CMSDatabase db, PageModel model, HttpContext context)
        {
            if (model == null)
                return null;
            if (!model.PageType.HasValue)
                return null;
            if (string.IsNullOrEmpty(model.Title) || string.IsNullOrEmpty(model.PageName))
                return null;

            Page page = null;
            switch (model.PageType.Value)
            {
                case PageType.Usual:
                    UsualPage usualPage = new UsualPage();
                    page = usualPage;
                    // Главной страницей может быть только та страница, у которой стоит галка isMainPage на форме,
                    // тип которой == PageType.Usual и которая не имеет страницы-родителя.
                    // Так же в БД не должно быть страницы, Url которой == "/"
                    if (model.IsMainPage && !model.PreviousPageID.HasValue && !HasMainPage(db))
                        model.Alias = "index";
                    // Если потенциальная главная страница не прошла какое-нибудь из условий, описанных выше, то
                    // возвращаем пользователю сообщение об ошибке
                    else if (model.IsMainPage)
                        return null;
                    if (model.PreviousPageID.HasValue)
                    {
                        usualPage.PreviousPage = db.UsualPages.FirstOrDefault(up => up.ID == model.PreviousPageID.Value);
                        if (usualPage.PreviousPage == null)
                            usualPage.PreviousPageID = null;
                    }
                    if (usualPage.PreviousPage == null || usualPage.PreviousPage.RequestPath.Equals("/", StringComparison.Ordinal))
                        usualPage.RequestPath = "/";
                    else usualPage.RequestPath = $"{usualPage.PreviousPage.RequestPath}/";
                    break;

                case PageType.Category:
                    // Т.к. категория не может быть главной страницей
                    if (model.IsMainPage)
                        return null;
                    CategoryPage categoryPage = new CategoryPage();
                    page = categoryPage;
                    if (model.PreviousPageID.HasValue)
                    {
                        categoryPage.PreviousPage = db.UsualPages.FirstOrDefault(up => up.ID == model.PreviousPageID.Value);
                        if (categoryPage.PreviousPage == null)
                            categoryPage.PreviousPageID = null;
                    }
                    if (categoryPage.PreviousPage == null || categoryPage.PreviousPage.RequestPath.Equals("/", StringComparison.Ordinal))
                        categoryPage.RequestPath = "/";
                    else categoryPage.RequestPath = $"{categoryPage.PreviousPage.RequestPath}/";
                    break;

                case PageType.Product:
                    // Т.к. продукт не может быть главной страницей
                    if (model.IsMainPage)
                        return null;
                    // Продукт всегда должен иметь страницу-родителя в виде категории
                    if (!model.PreviousPageID.HasValue)
                        return null;
                    ProductPage productPage = new ProductPage();
                    productPage.PreviousPage = db.CategoryPages.FirstOrDefault(cp => cp.ID == model.PreviousPageID); // ←
                    if (productPage.PreviousPage == null)
                        return null;
                    page = productPage;
                    productPage.Price = model.Price;
                    productPage.OldPrice = model.OldPrice;
                    productPage.ShortDescription = model.ShortDescription;
                    productPage.SpecialProduct = model.SpecialProduct;
                    productPage.RequestPath = $"{productPage.PreviousPage.RequestPath}/";
                    productPage.LastUpdate = DateTime.Now;
                    break;

                default:
                    return null;
            }
            page.Title = model.Title;
            page.PageName = model.PageName;

            // Если псевдоним страницы не указан, то переводим в транслит имя страницы
            // Если же псевдоним указан, то просто проверяем его на корректность
            if (string.IsNullOrEmpty(model.Alias))
                page.Alias = OtherFunctions.GetCorrectName(model.PageName, context);
            else
                page.Alias = OtherFunctions.GetCorrectName(model.Alias, context);
            // Если псевдоним содержал только некорректные символы (то есть после проверок он равен null),
            // тогда возвращаем пользователю сообщение об ошибке
            if (string.IsNullOrEmpty(page.Alias))
                return null;

            if (page.RequestPath.Equals("/") && page.Alias.Equals("index", StringComparison.Ordinal) && !model.IsMainPage)
                page.Alias = "ind";

            if (model.ID.HasValue)
                page.ID = model.ID.Value;

            if (!model.IsMainPage)
            {
                page.RequestPath += page.Alias;
                SetUniqueAliasName(db, page);
            }

            // Проверка на то, не присвоен ли запрещенный url текущей странице
            IHostingEnvironment env = context.RequestServices.GetRequiredService<IHostingEnvironment>();
            for (LinkedListNode<string> it = env.GetForbiddenUrls().First; it != null; it = it.Next)
            {
                if (page.RequestPath.Equals(it.Value, StringComparison.OrdinalIgnoreCase))
                {
                    page.Alias += "_page";
                    page.RequestPath += "_page";
                    SetUniqueAliasName(db, page);
                    it = env.GetForbiddenUrls().First;
                }
            }

            page.RequestPathHash = OtherFunctions.GetHashFromString(page.RequestPath);

            page.BreadcrumbsHtml = GetBreadcrumbsHTML(page);

            page.Content = model.Content;
            if (model.TemplateId.HasValue)
                page.Template = db.Templates.FirstOrDefault(t => t.ID == model.TemplateId);
            page.Published = model.Published;
            page.PageDescription = model.PageDescription;
            page.PageKeywords = model.PageKeywords;
            page.IsIndex = model.IsIndex;
            page.IsFollow = model.IsFollow;

            // Вставляем тег <p>, если стоит галка
            if (page is ProductPage pp && model.AddParagraphTag)
            {
                pp.Content = GetContentWithParagraphTag(pp.Content);
                pp.ShortDescription = GetContentWithParagraphTag(pp.ShortDescription);
            }

            return page;
        }
    }
}