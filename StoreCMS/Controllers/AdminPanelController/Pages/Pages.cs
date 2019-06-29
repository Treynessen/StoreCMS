using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Treynessen.AdminPanelTypes;
using Treynessen.PagesManagement;

namespace Treynessen.Controllers
{
    public partial class AdminPanelController : Controller
    {
        [NonAction]
        public IActionResult Pages()
        {
            HttpContext.Items["pageID"] = AdminPanelPages.Pages;

            var usualPages = db.UsualPages.AsNoTracking().Select(page => new PageViewModel { ID = page.ID, Name = page.PageName, Url = PagesManagementFunctions.GetUrl(page.RequestPathWithoutAlias, page.Alias), PageType = PageType.Usual });
            var categoryPages = db.CategoryPages.AsNoTracking().Select(page => new PageViewModel { ID = page.ID, Name = page.PageName, Url = PagesManagementFunctions.GetUrl(page.RequestPathWithoutAlias, page.Alias), PageType = PageType.Category });
            var sortedPages = usualPages.Concat(categoryPages).OrderBy(page => page.PageType).ThenBy(page => page.ID).ToArray();
            return View("Pages/Index", sortedPages);
        }
    }
}