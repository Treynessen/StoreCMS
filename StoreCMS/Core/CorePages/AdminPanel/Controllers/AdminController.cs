using Trane.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Trane.AdminPanel
{
    [Area("AdminPanel")]
    public class AdminController : Controller
    {
        private IConfiguration pathConfig;
        private ILocalization localization;

        public AdminController(Trane.Configurations.ConfigurationsContainer container,
            ILocalization localization)
        {
            pathConfig = container.PathsConfiguration;
            this.localization = localization;
        }

        [HttpGet]
        public IActionResult LoginForm()
        {
            return View("~"+ pathConfig["admin_views_path"] + "LoginForm.cshtml");
        }

        [HttpPost]
        public IActionResult LoginForm(LoginPasswordChecker checker)
        {
            if (!checker.Check())
            {
                ViewData["incorrectLoginPassword"] = localization.IncorrectLoginPassword;
                return LoginForm();
            }
            return AdminPanel();
        }

        public IActionResult AdminPanel()
        {
            return View("~" + pathConfig["admin_views_path"] + "AdminPanel.cshtml");
        }
    }
}