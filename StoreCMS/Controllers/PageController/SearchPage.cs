using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Treynessen.Database.Entities;
using Treynessen.Database.Queries;
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
                int id = Convert.ToInt32(config.GetConfigValue("SearchPageSettings:SearchPageTemplateId"));
                template = db.Templates.AsNoTracking().FirstOrDefault(t => t.ID == id);
                if (template == null) throw new FormatException();
            }
            catch (FormatException) { return PageNotFound(); }
            if (!string.IsNullOrEmpty(model.Search))
            {
                int maxSymbols;
                if (!int.TryParse(config.GetConfigValue("SearchPageSettings:MaxNumberOfSymbolsInSearchQuery"), out maxSymbols) || maxSymbols < 1) maxSymbols = 200;
                maxSymbols = Math.Min(maxSymbols, ProductSearch.MaxLength);
                if (model.Search.Length > maxSymbols)
                {
                    int length = maxSymbols;
                    if (char.IsHighSurrogate(model.Search[length - 1]) && char.IsLowSurrogate(model.Search[length])) length--;
                    model.Search = model.Search.Substring(0, length);
                }
                IQueryable<ProductPage> query;
                try { query = ProductSearch.Query(db, model.Search); }
                catch (SearchLimitException) { return BadRequest("Превышены допустимые ограничения поиска."); }
                int pageSize;
                if (!int.TryParse(config.GetConfigValue("CategoryPageSettings:NumberOfProductsOnPage"), out pageSize) || pageSize < 1) pageSize = 20;
                pageSize = Math.Min(pageSize, 100);
                int count = query.Count();
                int pages = GetPagesCount(count, pageSize);
                int page = model.Page.HasValue && model.Page.Value >= 1 && model.Page.Value <= pages ? model.Page.Value : 1;
                HttpContext.Items["SearchResultCount"] = count;
                HttpContext.Items["PagesCount"] = pages;
                HttpContext.Items["CurrentPage"] = page;
                HttpContext.Items["PaginationStyleName"] = config.GetConfigValue("CategoryPageSettings:PaginationStyleName");
                HttpContext.Items["OrderBy"] = model.Orderby;
                HttpContext.Items["products"] = query.OrderBy(p => p.ID).Skip((page - 1) * pageSize).Take(pageSize).ToList();
            }
            return View(template.TemplatePath);
        }
    }
}
