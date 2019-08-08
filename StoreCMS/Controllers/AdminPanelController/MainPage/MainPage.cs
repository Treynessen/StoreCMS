using Microsoft.AspNetCore.Mvc;
using Treynessen.AdminPanelTypes;

namespace Treynessen.Controllers
{
    public partial class AdminPanelController : Controller
    {
        [NonAction]
        public IActionResult MainPage()
        {
            HttpContext.Items["pageID"] = AdminPanelPages.MainPage;
            return View("MainPage/MainPage");
        }
    }
}