using Microsoft.AspNetCore.Mvc;
using Treynessen.Database.Context;

namespace Treynessen.Controllers
{
    public partial class AdminPanelController : Controller
    {
        private CMSDatabase db;

        public AdminPanelController(CMSDatabase db)
        {
            this.db = db;
        }
    }
}