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

        public IActionResult Index()
        {
            ViewData["title"] = localization.AdminPanelTitle;
            return View("~"+ pathConfig["admin_views_path"] + "Index.cshtml");
        }
    }
}
