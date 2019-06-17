using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Treynessen.Functions;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;

namespace Treynessen.RouteConstraints
{
    public class UrlConstraint : IRouteConstraint
    {
        public bool Match(HttpContext httpContext, IRouter route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
        {
            CMSDatabase db = httpContext.RequestServices.GetRequiredService<CMSDatabase>();
            Page page = null;
            string path = httpContext.Request.Path;
            if (path[path.Length - 1] == '/')
                path = path.Substring(0, path.Length - 1);
            if (string.IsNullOrEmpty(path))
            {
                page = db.UsualPages.FirstOrDefaultAsync(up => !up.PreviousPageID.HasValue && up.Alias.Equals("index", StringComparison.InvariantCulture)).Result;
            }
            else
            {
                Task<UsualPage> usualPageTask = db.UsualPages.FirstOrDefaultAsync(up => OtherFunctions.GetUrl(up).Equals(path, StringComparison.InvariantCulture));
                Task<CategoryPage> categoryPageTask = db.CategoryPages.FirstOrDefaultAsync(cp => OtherFunctions.GetUrl(cp).Equals(path, StringComparison.InvariantCulture));
                Task<ProductPage> productPageTask = null;
                if (path.Split('/').Length >= 3)
                {
                    productPageTask = db.ProductPages.FirstOrDefaultAsync(pp => OtherFunctions.GetUrl(pp).Equals(path, StringComparison.InvariantCulture));
                }
                Task.WaitAll(usualPageTask, categoryPageTask);
                if (productPageTask != null)
                {
                    if (productPageTask.Result != null)
                        page = productPageTask.Result;
                }
                if (page == null && categoryPageTask.Result != null)
                    page = categoryPageTask.Result;
                else if (page == null && usualPageTask.Result != null)
                    page = usualPageTask.Result;
            }
            if (page != null)
            {
                httpContext.Items["PageObject"] = page;
                return true;
            }
            return false;
        }
    }
}
