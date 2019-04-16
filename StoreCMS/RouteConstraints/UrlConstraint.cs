using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Treynessen.Functions;
using Treynessen.AdminPanelTypes;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;

namespace Treynessen.RouteConstraints
{
    public class UrlConstraint : IRouteConstraint
    {
        private PageType pageType;

        public UrlConstraint(PageType pageType)
            => this.pageType = pageType;

        public bool Match(HttpContext httpContext, IRouter route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
        {
            CMSDatabase db = httpContext.RequestServices.GetRequiredService<CMSDatabase>();
            Page page = null;
            string path = httpContext.Request.Path;
            if (path[path.Length - 1] == '/')
                path = path.Substring(0, path.Length - 1);
            switch (pageType)
            {
                case PageType.Usual:
                    page = db.UsualPages.FirstOrDefaultAsync(up => OtherFunctions.GetUrl(up).Equals(path)).Result;
                    if (page != null)
                    {
                        httpContext.Items["PageObject"] = page;
                        return true;
                    }
                    break;
                case PageType.Category:
                    page = db.CategoryPages.FirstOrDefaultAsync(cp => OtherFunctions.GetUrl(cp).Equals(path)).Result;
                    if (page != null)
                    {
                        httpContext.Items["PageObject"] = page;
                        return true;
                    }
                    break;
                case PageType.Product:
                    break;
            }
            return false;
        }
    }
}
