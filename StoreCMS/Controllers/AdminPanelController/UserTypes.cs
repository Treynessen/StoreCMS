using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Treynessen.AdminPanelTypes;

namespace Treynessen.Controllers
{
    public partial class AdminPanelController : Controller
    {
        [NonAction]
        public IActionResult UserTypes()
        {
            HttpContext.Items["pageID"] = AdminPanelPages.UserTypes;
            return View("UserTypes", db.UserTypes.AsNoTracking().ToArray());
        }
    }
}