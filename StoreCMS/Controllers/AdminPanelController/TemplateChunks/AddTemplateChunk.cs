using Microsoft.AspNetCore.Mvc;
using Treynessen.AdminPanelTypes;

namespace Treynessen.Controllers
{
    public partial class AdminPanelController : Controller
    {
        [NonAction]
        public IActionResult AddTemplateChunk(TemplateModel model = null)
        {
            return View("Templates/AddTemplate", model);
        }
    }
}