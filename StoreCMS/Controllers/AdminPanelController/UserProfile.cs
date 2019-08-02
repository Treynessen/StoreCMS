using Microsoft.AspNetCore.Mvc;
using Treynessen.AdminPanelTypes;

namespace Treynessen.Controllers
{
    public partial class AdminPanelController : Controller
    {
        [NonAction]
        public IActionResult UserProfile()
        {
            HttpContext.Items["pageID"] = AdminPanelPages.UserProfile;
            return View("UserProfile", HttpContext.Items["User"]);
        }
    }
}