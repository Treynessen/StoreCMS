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
        [HttpGet]
        public IActionResult SearchPage(PageControllerModel model)
        {
            ConfigurationHandler config = HttpContext.RequestServices.GetRequiredService<ConfigurationHandler>();
            Template template = null;
            try
            {
                int pageNotFoundTemplateId = Convert.ToInt32(config.GetConfigValue("SearchPageSettings:SearchPageTemplateId"));
                template = db.Templates.AsNoTracking().FirstOrDefault(t => t.ID == pageNotFoundTemplateId);
                if (template == null)
                    throw new FormatException();
            }
            catch (FormatException)
            {
                return PageNotFound();
            }
            int maxSymbols = 0;
            try
            {
                maxSymbols = Convert.ToInt32(config.GetConfigValue("SearchPageSettings:MaxNumberOfSymbolsInSearchQuery"));
            }
            catch (FormatException)
            {
                maxSymbols = 0;
            }
            if (!string.IsNullOrEmpty(model.Search))
            {
                if (model.Search.Length > maxSymbols)
                    model.Search = model.Search.Substring(0, maxSymbols);
                // Создаем список из всех возможных вариаций текущего запроса, основываясь на заданных синонимах в таблице SynonymsForStrings
                LinkedList<string> searchQueryList = new LinkedList<string>();
                searchQueryList.AddLast(model.Search);
                SynonymForString[] synonymsForStrings = db.SynonymsForStrings.Where(s => model.Search.Contains(s.String, StringComparison.InvariantCultureIgnoreCase)
                || model.Search.Contains(s.Synonym, StringComparison.InvariantCultureIgnoreCase)).AsNoTracking().ToArray();
                foreach (var synonymForString in synonymsForStrings)
                {
                    for (var it = searchQueryList.First; it != null; it = it?.Next)
                    {
                        if (it.Value.Contains(synonymForString.String, StringComparison.InvariantCultureIgnoreCase))
                        {
                            searchQueryList.AddFirst(it.Value.Replace(synonymForString.String, synonymForString.Synonym, StringComparison.InvariantCultureIgnoreCase));
                        }
                        else if (it.Value.Contains(synonymForString.Synonym, StringComparison.InvariantCultureIgnoreCase))
                        {
                            searchQueryList.AddFirst(it.Value.Replace(synonymForString.Synonym, synonymForString.String, StringComparison.InvariantCultureIgnoreCase));
                        }
                    }
                }
                // Создаем запрос к бд для каждого поиского запроса
                LinkedList<IQueryable<ProductPage>> queries = new LinkedList<IQueryable<ProductPage>>();
                foreach (var searchQuery in searchQueryList)
                {
                    string[] subSearchQuery = searchQuery.Split(' ');
                    IQueryable<ProductPage> query = null;
                    foreach (var s in subSearchQuery)
                    {
                        if (query == null)
                            query = db.ProductPages.Where(pp => pp.PageName.Contains(s, StringComparison.InvariantCultureIgnoreCase));
                        else query = query.Where(pp => pp.PageName.Contains(s, StringComparison.InvariantCultureIgnoreCase));
                    }
                    if (query != null)
                        queries.AddLast(query);
                }
                if (queries.Count > 0)
                {
                    // Объединяем полученные запросы к бд через Union, чтобы избежать дубликатов
                    IQueryable<ProductPage> finalQuery = queries.First.Value;
                    for (var it = queries.First.Next; it != null; it = it.Next)
                    {
                        finalQuery = finalQuery.Union(it.Value);
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
                        int pagesCount = GetPagesCount(finalQuery.Count(), maxProductOnPage.Value);
                        HttpContext.Items["PagesCount"] = pagesCount;
                        HttpContext.Items["PaginationStyleName"] = config.GetConfigValue("CategoryPageSettings:PaginationStyleName");
                        HttpContext.Items["OrderBy"] = model.Orderby;
                        //Если задана страница, то скипаем предыдущие товары для дальнейшего отбора
                        if (model.Page.HasValue && model.Page.Value > 1 && model.Page.Value <= pagesCount)
                        {
                            HttpContext.Items["CurrentPage"] = model.Page.Value;
                            finalQuery = finalQuery.Skip((model.Page.Value - 1) * maxProductOnPage.Value);
                        }
                        if (HttpContext.Items["CurrentPage"] == null)
                            HttpContext.Items["CurrentPage"] = 1;
                        finalQuery = finalQuery.Take(maxProductOnPage.Value);
                    }
                    else
                    {
                        HttpContext.Items["CurrentPage"] = 1;
                        HttpContext.Items["PagesCount"] = 1;
                    }

                    // Получаем список найденных товаров
                    HttpContext.Items["products"] = finalQuery.AsNoTracking().ToList();
                }
            }
            return View(template.TemplatePath);
        }
    }
}