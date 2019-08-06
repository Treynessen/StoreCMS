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
        public IActionResult RequestHandler(PageControllerModel model)
        {
            Page page = HttpContext.Items["RequestedPage"] as Page;
            switch (page)
            {
                case UsualPage up:
                    SetVisitInfo(up.ID, PagesManagement.PageType.Usual);
                    return UsualPage(up);
                case CategoryPage cp:
                    SetVisitInfo(cp.ID, PagesManagement.PageType.Category);
                    return CategoryPage(cp, model);
                case ProductPage pp:
                    SetVisitInfo(pp.ID, PagesManagement.PageType.Product);
                    return ProductPage(pp);
                default:
                    return Redirect(HttpContext.Items["Redirection"] as string);
            }
        }
    }
}