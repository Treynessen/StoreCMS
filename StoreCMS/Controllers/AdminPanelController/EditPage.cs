using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Treynessen.Functions;
using Treynessen.AdminPanelTypes;
using Treynessen.Database.Entities;

namespace Treynessen.Controllers
{
    public partial class AdminPanelController : Controller
    {
        [NonAction]
        public IActionResult EditPage(PageType? pageType, int? itemID, PageModel model = null)
        {
            if (!pageType.HasValue || !itemID.HasValue)
                return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.Pages}");
            if (model == null)
            {
                Page page = null;
                switch (pageType.Value)
                {
                    case PageType.Usual:
                        page = db.UsualPages.FirstOrDefaultAsync(up => up.ID == itemID.Value).Result;
                        break;
                    case PageType.Category:
                        page = db.CategoryPages.FirstOrDefaultAsync(cp => cp.ID == itemID.Value).Result;
                        break;
                }
                model = OtherFunctions.PageToPageModel(page);
            }
            if (model == null)
                return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.Pages}");
            SetRoutes("EditPage");
            return View("Pages/EditPage", model);
        }
    }
}