using Trane.Db.Context;
using Trane.Db.Entities;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Trane.RouteConstraints
{
    public class SimplePageConstraint : IRouteConstraint
    {
        private DbSet<SimplePage> simplePages;

        public SimplePageConstraint(CMSContext db)
        {
            simplePages = db.SimplePages;
        }

        public bool Match(HttpContext httpContext, IRouter route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
        {
            
            return true;
        }
    }
}
