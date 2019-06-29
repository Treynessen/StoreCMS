using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Treynessen.AdminPanelTypes;
using Treynessen.TemplatesManagement;

namespace Treynessen.Controllers
{
    public partial class AdminPanelController : Controller
    {
        [NonAction]
        public IActionResult EditChunk(int? itemID, TemplateModel model = null)
        {
            HttpContext.Items["pageID"] = AdminPanelPages.EditChunk;
            if (!itemID.HasValue)
                return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.Chunks}");
            if (model == null)
                model = TemplatesManagementFunctions.ITemplateToTemplateModel(db.Chunks.FirstOrDefaultAsync(p => p.ID == itemID.Value).Result);
            else
                HttpContext.Items["IsIncorrect"] = true;
            if (model == null)
                return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.Chunks}");
            return View("Templates/AddOrEditTemplate", model);
        }
    }
}