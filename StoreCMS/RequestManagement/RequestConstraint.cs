using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;

namespace Treynessen.RequestManagement
{
    public class RequestConstraint : IRouteConstraint
    {
        public bool Match(HttpContext httpContext, IRouter route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
        {
            CMSDatabase db = httpContext.RequestServices.GetRequiredService<CMSDatabase>();
            RequestHandler requestHandler = new RequestHandler(db, httpContext.Request.Path);
            Page page = requestHandler.FindPage();
            if (page != null)
            {
                httpContext.Items["RequestedPage"] = page;
                return true;
            }
            else return false;
        }
    }
}