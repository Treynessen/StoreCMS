using Treynessen.Localization;
using Microsoft.AspNetCore.Mvc;
using Treynessen.Database.Context;

namespace Treynessen.Controllers
{
    public partial class AdminPanelController : Controller
    {
        private CMSDatabase db;
        private IAdminPanelLogLocalization localization;

        public AdminPanelController(CMSDatabase db, IAdminPanelLogLocalization localization)
        {
            this.db = db;
            this.localization = localization;
        }
    }
}