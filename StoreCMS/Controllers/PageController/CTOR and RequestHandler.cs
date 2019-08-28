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
                    try
                    {
                        SetVisitInfo(up.ID, PagesManagement.PageType.Usual);
                    }
                    catch { }
                    return UsualPage(up);
                case CategoryPage cp:
                    try
                    {
                        SetVisitInfo(cp.ID, PagesManagement.PageType.Category);
                    }
                    catch { }
                    return CategoryPage(cp, model);
                case ProductPage pp:
                    try
                    {
                        SetVisitInfo(pp.ID, PagesManagement.PageType.Product);
                    }
                    catch { }
                    return ProductPage(pp);
                default:
                    return Redirect(HttpContext.Items["Redirection"] as string);
            }
        }
    }
}