using Microsoft.AspNetCore.Mvc;

namespace Treynessen.Controllers
{
    public partial class AdminPanelController : Controller
    {
        [NonAction]
        public IActionResult Pages()
        {
            SetRoutes("Pages");
            return View("Pages/Index");
        }
    }
}