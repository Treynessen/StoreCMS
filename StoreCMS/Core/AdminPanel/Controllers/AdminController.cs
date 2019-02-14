using Trane.Localization;
using Trane.AdminPanel;
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
        public IActionResult Index()
        {
            ViewData["title"] = localization.AdminPanelTitle;
            ViewData["userNameLabel"] = localization.UserNameLabel;
            ViewData["passwordLabel"] = localization.PasswordLabel;
            ViewData["loginButtonText"] = localization.LoginButtonText;
            return View("~"+ pathConfig["admin_views_path"] + "Index.cshtml");
        }

        [HttpPost]
        public IActionResult Index(LoginPasswordChecker checker)
        {
            if (checker.Check()) return Content("Админ панель");
            return Content($"Неверно введены логин и пароль");
        }
    }
}
