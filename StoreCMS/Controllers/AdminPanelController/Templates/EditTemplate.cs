using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Treynessen.AdminPanelTypes;
using Treynessen.TemplatesManagement;

namespace Treynessen.Controllers
{
    public partial class AdminPanelController : Controller
    {
        [NonAction]
        public IActionResult EditTemplate(int? itemID, TemplateModel model = null)
        {
            HttpContext.Items["pageID"] = AdminPanelPages.EditTemplate;
            if (!itemID.HasValue)
                return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.Templates}");
            if (model == null)
                model = TemplatesManagementFunctions.ITemplateToTemplateModel(
                    template: db.Templates.FirstOrDefaultAsync(p => p.ID == itemID.Value).Result
                );
            else
                HttpContext.Items["IsIncorrect"] = true;
            if (model == null)
                return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.Templates}");
            return View("Templates/AddOrEditTemplate", model);
        }
    }
}