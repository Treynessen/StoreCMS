using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Treynessen.Controllers
{
    public partial class AdminPanelController : Controller
    {
        [NonAction]
        public IActionResult Templates()
        {
            return View("Templates/TemplatesPage", db.Templates.AsNoTracking().ToArray());
        }
    }
}