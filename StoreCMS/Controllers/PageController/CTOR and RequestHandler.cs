using Microsoft.AspNetCore.Mvc;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;

namespace Treynessen.Controllers
{
    public partial class PageController : Controller
    {
        private CMSDatabase db;

        public PageController(CMSDatabase db)
        {
            this.db = db;
        }

        [HttpGet]
        public IActionResult RequestHandler()
        {
            Page page = HttpContext.Items["RequestedPage"] as Page;
            switch (page)
            {
                case UsualPage up:
                    return UsualPage(up);
                case CategoryPage cp:
                    return CategoryPage(cp);
                case ProductPage pp:
                    return ProductPage(pp);
                default:
                    return null;
            }
        }
    }
}