using Microsoft.AspNetCore.Mvc;

namespace Treynessen.Controllers
{
    public partial class AdminPanelController : Controller
    {
        [NonAction]
        public IActionResult Templates()
        {
            SetRoutes("Templates");
            return View("Templates");
        }
    }
}