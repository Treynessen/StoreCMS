using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
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
                LinkedList<string> searchQueryList = new LinkedList<string>();
                searchQueryList.AddLast(model.Search);
                // Получаем все выражения и синонимы, которые встретились в поисковом запросе. Создаем для каждого выражения и синонима регулярное выражение
                var synonymsForStringsWithRegex = db.SynonymsForStrings.Where(s => model.Search.Contains(s.String, StringComparison.InvariantCultureIgnoreCase)
                || model.Search.Contains(s.Synonym, StringComparison.InvariantCultureIgnoreCase)).AsNoTracking()
                .Select(s => new
                {
                    s.String,
                    s.Synonym,
                    StringRegex = new Regex($"(?(^{s.String})" +
                        $"(?(^{s.String}$)(^{s.String}$)|(^{s.String} ))" + // если проверяемое выражение начинается и заканчивается со String или, если начинает, но не заканчивается на String
                        $"|(?( {s.String}$)( {s.String}$)|( {s.String} ))" + // если проверяемое выражение не начинается со String, но заканчивается на нем или, если не начинает и не заканчивается на String
                        ")", 
                        RegexOptions.IgnoreCase
                    ),
                    SynonymRegex = new Regex($"(?(^{s.Synonym})" + // так же, как и сверху, только для Synonym
                        $"(?(^{s.Synonym}$)(^{s.Synonym}$)|(^{s.Synonym} ))" +
                        $"|(?( {s.Synonym}$)( {s.Synonym}$)|( {s.Synonym} ))" +
                        ")",
                        RegexOptions.IgnoreCase
                    )
                }).ToArray();
                // На основе всех совпадений из таблицы SynonymsForStrings создаем все возможные вариации текущего запроса
                foreach (var synonymForStringWithRegex in synonymsForStringsWithRegex)
                {
                    for (var it = searchQueryList.First; it != null; it = it?.Next)
                    {
                        if (synonymForStringWithRegex.StringRegex.IsMatch(it.Value))
                        {
                            searchQueryList.AddFirst(it.Value.Replace(synonymForStringWithRegex.String, synonymForStringWithRegex.Synonym, StringComparison.InvariantCultureIgnoreCase));
                        }
                        else if (synonymForStringWithRegex.SynonymRegex.IsMatch(it.Value))
                        {
                            searchQueryList.AddFirst(it.Value.Replace(synonymForStringWithRegex.Synonym, synonymForStringWithRegex.String, StringComparison.InvariantCultureIgnoreCase));
                        }
                    }
                }
                // Создаем запрос к бд для каждого поиского запроса
                LinkedList<IQueryable<ProductPage>> queries = new LinkedList<IQueryable<ProductPage>>();
                foreach (var searchQuery in searchQueryList)
                {
                    string[] subSearchQuery = searchQuery.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    IQueryable<ProductPage> query = null;
                    foreach (var s in subSearchQuery)
                    {
                        if (query == null)
                            query = db.ProductPages.Where(pp => pp.Published && pp.PageName.Contains(s, StringComparison.InvariantCultureIgnoreCase));
                        else query = query.Where(pp => pp.Published && pp.PageName.Contains(s, StringComparison.InvariantCultureIgnoreCase));
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
                        finalQuery = finalQuery.Union(it.Value).OrderBy(pp => pp.ID);
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