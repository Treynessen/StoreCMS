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
        public IActionResult EditChunk(int? itemID)
        {
            HttpContext.Items["pageID"] = AdminPanelPages.EditChunk;
            if (!itemID.HasValue)
                return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.Chunks}");
            Chunk chunk = db.Chunks.AsNoTracking().FirstOrDefault(p => p.ID == itemID.Value);
            TemplateModel model = TemplatesManagementFunctions.ITemplateToTemplateModel(chunk);
            if (model == null)
                return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.Chunks}");
            return View("Chunks/EditChunk", model);
        }
    }
}