using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Treynessen.Functions;
using Treynessen.AdminPanelTypes;

namespace Treynessen.Controllers
{
    public partial class AdminPanelController : Controller
    {
        [NonAction]
        public IActionResult EditTemplate(int? itemID, TemplateModel model = null)
        {
            SetRoutes("EditTemplate");

            if(!itemID.HasValue)
                return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.Templates}");
            if (model == null)
                model = OtherFunctions.TemplateToTemplateModel(db.Templates.FirstOrDefaultAsync(p => p.ID == itemID.Value).Result);
            else
                HttpContext.Items["IsIncorrect"] = true;
            if (model == null)
                return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.Templates}");
            return View("Templates/EditTemplate", model);
        }
    }
}