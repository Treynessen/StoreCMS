using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Treynessen.AdminPanelTypes;
using Treynessen.Database.Entities;
using Treynessen.TemplatesManagement;

namespace Treynessen.Controllers
{
    public partial class AdminPanelController : Controller
    {
        [NonAction]
        public IActionResult EditTemplate(int? itemID)
        {
            HttpContext.Items["pageID"] = AdminPanelPages.EditTemplate;
            if (!itemID.HasValue)
                return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.Templates}");
            Template template = db.Templates.AsNoTracking().FirstOrDefault(p => p.ID == itemID.Value);
            TemplateModel model = TemplatesManagementFunctions.ITemplateToTemplateModel(template);
            if (model == null)
                return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.Templates}");
            return View("Templates/EditTemplate", model);
        }
    }
}