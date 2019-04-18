using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace Treynessen.Controllers
{
    public partial class AdminPanelController : Controller
    {
        [NonAction]
        public IActionResult TemplateChunks()
        {
            SetRoutes("TemplateChunks");
            return View("Templates/ChunksPage", db.TemplateChunks.ToArray());
        }
    }
}