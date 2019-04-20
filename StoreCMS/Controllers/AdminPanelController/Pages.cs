using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Treynessen.Functions;
using Treynessen.AdminPanelTypes;

namespace Treynessen.Controllers
{
    public partial class AdminPanelController : Controller
    {
        [NonAction]
        public IActionResult Pages()
        {
            SetRoutes("Pages");
            var usualPages = db.UsualPages.Select(page => new PageViewModel { ID = page.ID, Title = page.Title, Url = OtherFunctions.GetUrl(page.RequestPathWithoutAlias, page.Alias), PageType = PageType.Usual });
            var categoryPages = db.CategoryPages.Select(page => new PageViewModel { ID = page.ID, Title = page.Title, Url = OtherFunctions.GetUrl(page.RequestPathWithoutAlias, page.Alias), PageType = PageType.Category });
            var sortedPages = usualPages.Concat(categoryPages).OrderBy(page => page.PageType).ThenBy(page => page.ID).ToList();
            return View("Pages/Index", sortedPages);
        }
    }
}