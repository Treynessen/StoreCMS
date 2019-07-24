using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Treynessen.AdminPanelTypes;

namespace Treynessen.Controllers
{
    public partial class AdminPanelController : Controller
    {
        [NonAction]
        public IActionResult Redirections()
        {
            HttpContext.Items["pageID"] = AdminPanelPages.Redirections;
            return View("Redirections", db.Redirections.AsNoTracking().ToArray());
        }
    }
}