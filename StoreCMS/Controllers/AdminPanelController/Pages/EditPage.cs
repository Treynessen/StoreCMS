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
        public IActionResult EditPage(int? itemID)
        {
            HttpContext.Items["pageID"] = AdminPanelPages.EditPage;

            if (!itemID.HasValue)
                return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.Pages}");
            UsualPage page = db.UsualPages.FirstOrDefaultAsync(up => up.ID == itemID.Value).Result;
            if (page == null)
                return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.Pages}");
            // Для блокировки выбора страницы-родителя в представлении
            HttpContext.Items["isMainPage"] = page.RequestPath.Equals("/", StringComparison.InvariantCulture);
            db.Entry(page).State = EntityState.Detached;
            PageModel model = PagesManagementFunctions.PageToPageModel(page);
            HttpContext.Items["UsualPages"] = db.UsualPages.ToArrayAsync().Result;
            HttpContext.Items["Templates"] = db.Templates.ToArrayAsync().Result;
            return View("Pages/EditPage", model);
        }
    }
}