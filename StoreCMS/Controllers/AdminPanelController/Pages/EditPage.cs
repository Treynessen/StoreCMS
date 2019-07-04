using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Treynessen.PagesManagement;
using Treynessen.AdminPanelTypes;
using Treynessen.Database.Entities;

namespace Treynessen.Controllers
{
    public partial class AdminPanelController : Controller
    {
        [NonAction]
        public IActionResult EditPage(PageType? pageType, int? itemID, PageModel model = null)
        {
            HttpContext.Items["pageID"] = AdminPanelPages.EditPage;

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
                    default:
                        return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.Pages}");
                }
                if (page != null)
                {
                    // Для блокировки выбора страницы-родителя в представлении
                    HttpContext.Items["isMainPage"] = page.RequestPath.Equals("/", StringComparison.InvariantCulture);
                    db.Entry(page).State = EntityState.Detached;
                    model = PagesManagementFunctions.PageToPageModel(page);
                }
            }
            else
                HttpContext.Items["IsIncorrect"] = true;
            if (model == null)
                return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.Pages}");
            return View("Pages/EditPage", model);
        }
    }
}