using Microsoft.AspNetCore.Mvc;
using Treynessen.AdminPanelTypes;

namespace Treynessen.Controllers
{
    public partial class AdminPanelController : Controller
    {
        [NonAction]
        public IActionResult LoginForm(LoginFormModel model = null)
        {
            return View("LoginForm", model);
        }
    }
}