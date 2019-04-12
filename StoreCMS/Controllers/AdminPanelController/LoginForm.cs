using Microsoft.AspNetCore.Mvc;
using Treynessen.AdminPanelTypes;

namespace Treynessen.Controllers
{
    public partial class AdminPanelController : Controller
    {
        [NonAction]
        public IActionResult LoginForm(LoginFormModel model = null)
        {
            SetRoutes("LoginForm");
            return View("LoginForm", model);
        }
    }
}