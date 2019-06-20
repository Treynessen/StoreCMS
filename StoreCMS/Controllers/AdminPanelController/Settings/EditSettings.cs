using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Treynessen.Security;
using Treynessen.Functions;
using Treynessen.AdminPanelTypes;

namespace Treynessen.Controllers
{
    public partial class AdminPanelController : Controller
    {
        [NonAction]
        public void EditSettings(SettingsModel settingsModel)
        {
            AccessLevelConfiguration accessLevelConfiguration = HttpContext.Items["AccessLevelConfiguration"] as AccessLevelConfiguration;
            accessLevelConfiguration.ReplaceJsonWithSettingModel(settingsModel.AccessSettingsModel);
            IHostingEnvironment env = HttpContext.RequestServices.GetService<IHostingEnvironment>();
            OtherFunctions.SaveProductTemplate(settingsModel.ProductTemplate, env);
        }
    }
}