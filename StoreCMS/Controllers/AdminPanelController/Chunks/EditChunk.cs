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
        public IActionResult EditChunk(int? itemID)
        {
            HttpContext.Items["pageID"] = AdminPanelPages.EditChunk;
            if (!itemID.HasValue)
                return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.Chunks}");
            Chunk chunk = db.Chunks.FirstOrDefaultAsync(p => p.ID == itemID.Value).Result;
            TemplateModel model = TemplatesManagementFunctions.ITemplateToTemplateModel(chunk);
            if (model == null)
                return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.Chunks}");
            db.Entry(chunk).State = EntityState.Detached;
            return View("Chunks/EditChunk", model);
        }
    }
}