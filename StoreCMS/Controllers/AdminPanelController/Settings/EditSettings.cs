using Microsoft.AspNetCore.Mvc;
using Treynessen.Security;
using Treynessen.AdminPanelTypes;

namespace Treynessen.Controllers
{
    public partial class AdminPanelController : Controller
    {
        [NonAction]
        public void EditSettings(SettingsModel settingsModel)
        {
            AccessLevelConfiguration accessLevelConfiguration = HttpContext.Items["AccessLevelConfiguration"] as AccessLevelConfiguration;
            accessLevelConfiguration.ReplaceJsonWithSettingModel(settingsModel);
        }
    }
}