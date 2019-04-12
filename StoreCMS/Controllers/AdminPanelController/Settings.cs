using Microsoft.AspNetCore.Mvc;

namespace Treynessen.Controllers
{
    public partial class AdminPanelController : Controller
    {
        [NonAction]
        public IActionResult Settings()
        {
            SetRoutes("Settings");
            return View("Settings");
        }
    }
}