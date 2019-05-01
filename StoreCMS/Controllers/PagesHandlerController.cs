using Microsoft.AspNetCore.Mvc;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;

namespace Treynessen.Controllers
{
    public partial class PagesHandlerController : Controller
    {
        private CMSDatabase db;

        public PagesHandlerController(CMSDatabase db)
        {
            this.db = db;
        }

        [HttpGet]
        public IActionResult RequestHandler()
        {
            Page page = HttpContext.Items["PageObject"] as Page;
            switch (page)
            {
                case UsualPage up:
                    return UsualPage(up);
                case CategoryPage cp:
                    return CategoryPage(cp);
                case ProductPage pp:
                    return ProductPage(pp);
                default:
                    return StatusCode(404);
            }
        }
    }
}