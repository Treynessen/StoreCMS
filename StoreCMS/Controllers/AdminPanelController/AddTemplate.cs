using Microsoft.AspNetCore.Mvc;
using Treynessen.AdminPanelTypes;

namespace Treynessen.Controllers
{
    public partial class AdminPanelController : Controller
    {
        [NonAction]
        public IActionResult AddTemplate(TemplateModel model = null)
        {
            SetRoutes("AddTemplate");
            return View("Templates/AddTemplate", model);
        }
    }
}