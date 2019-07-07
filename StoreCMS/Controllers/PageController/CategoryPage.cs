using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Treynessen.Database.Entities;
using Treynessen.SettingsManagement;

namespace Treynessen.Controllers
{
    public partial class PageController : Controller
    {
        [NonAction]
        public IActionResult CategoryPage(CategoryPage categoryPage, PageControllerModel model)
        {
            db.Entry(categoryPage).Reference(cp => cp.Template).LoadAsync().Wait();
            ConfigurationHandler config = HttpContext.RequestServices.GetService<ConfigurationHandler>();
            // Получаем список продуктов
            IQueryable<ProductPage> categoryProducts = db.ProductPages.Where(pp => pp.PreviousPageID == categoryPage.ID);
            // Производим сортировку, если задано значение
            if (model.Orderby.HasValue)
            {
                switch (model.Orderby)
                {
                    case OrderBy.ascending_price:
                        categoryProducts = categoryProducts.OrderBy(pp => pp.Price);
                        break;
                    case OrderBy.descending_price:
                        categoryProducts = categoryProducts.OrderByDescending(pp => pp.Price);
                        break;
                }
            }
            // Получаем количество товаров на странице
            int? maxProductOnPage = null;
            try
            {
                maxProductOnPage = Convert.ToInt32(config.GetConfigValue("CategoryPageSettings:NumberOfProductsOnPage"));
            }
            catch (FormatException) { }
            // Если значение верное, то отбираем заданное количество товаров
            if (maxProductOnPage.HasValue && maxProductOnPage.Value >= 1)
            {
                int pagesCount = GetPagesCount(categoryPage.ProductsCount, maxProductOnPage.Value);
                HttpContext.Items["PagesCount"] = pagesCount;
                HttpContext.Items["PaginationStyleName"] = config.GetConfigValue("CategoryPageSettings:PaginationStyleName");
                HttpContext.Items["OrderBy"] = model.Orderby;
                //Если задана страница, то скипаем предыдущие товары для дальнейшего отбора
                if (model.Page.HasValue && model.Page.Value > 1 && model.Page.Value <= pagesCount)
                {
                    HttpContext.Items["CurrentPage"] = model.Page.Value;
                    categoryProducts = categoryProducts.Skip((model.Page.Value - 1) * maxProductOnPage.Value);
                }
                if (HttpContext.Items["CurrentPage"] == null)
                    HttpContext.Items["CurrentPage"] = 1;
                categoryProducts = categoryProducts.Take(maxProductOnPage.Value);
            }
            else
            {
                HttpContext.Items["CurrentPage"] = 1;
                HttpContext.Items["PagesCount"] = 1;
            }

            List<ProductPage> products = categoryProducts.ToListAsync().Result;
            HttpContext.Items["products"] = products;

            if (categoryPage.Template != null)
            {
                return View(categoryPage.Template.TemplatePath, categoryPage);
            }
            return Content(categoryPage.Content);
        }
    }
}