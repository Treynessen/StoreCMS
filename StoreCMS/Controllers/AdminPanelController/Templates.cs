using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Treynessen.Controllers
{
    public partial class AdminPanelController : Controller
    {
        [NonAction]
        public IActionResult Templates()
        {
            SetRoutes("Templates");
            return View("Templates/Index", db.Templates.ToListAsync().Result);
        }
    }
}