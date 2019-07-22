using System;
using System.Linq;
using System.Collections.Generic;
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
            UsualPage page = db.UsualPages.FirstOrDefault(up => up.ID == itemID.Value);
            if (page == null)
                return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.Pages}");
            // Получаем список ID страниц, которые зависят от текущей
            LinkedList<int> idList = PagesManagementFunctions.GetDependentPageIDs(db, page);
            // Для блокировки выбора страницы-родителя в представлении, если текущая страница - главная
            HttpContext.Items["isMainPage"] = page.RequestPath.Equals("/", StringComparison.InvariantCulture);
            PageModel model = PagesManagementFunctions.PageToPageModel(page);
            HttpContext.Items["UsualPages"] = db.UsualPages.AsNoTracking().Where(up => !idList.Contains(up.ID)).ToArray();
            HttpContext.Items["Templates"] = db.Templates.AsNoTracking().ToArray();
            return View("Pages/EditPage", model);
        }
    }
}