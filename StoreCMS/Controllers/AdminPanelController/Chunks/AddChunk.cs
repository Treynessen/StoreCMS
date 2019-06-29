using Microsoft.AspNetCore.Mvc;
using Treynessen.AdminPanelTypes;

namespace Treynessen.Controllers
{
    public partial class AdminPanelController : Controller
    {
        [NonAction]
        public IActionResult AddChunk(TemplateModel model = null)
        {
            HttpContext.Items["pageID"] = AdminPanelPages.AddChunk;
            if (model != null)
                HttpContext.Items["IsIncorrect"] = true;
            return View("Templates/AddOrEditTemplate", model);
        }
    }
}