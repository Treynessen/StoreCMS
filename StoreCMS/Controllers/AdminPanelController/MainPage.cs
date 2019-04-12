using Microsoft.AspNetCore.Mvc;

namespace Treynessen.Controllers
{
    public partial class AdminPanelController : Controller
    {
        [NonAction]
        public IActionResult MainPage()
        {
            SetRoutes("MainPage");
            return View("MainPage");
        }
    }
}