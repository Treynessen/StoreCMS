using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Treynessen.AdminPanelTypes;

namespace Treynessen.Controllers
{
    public partial class AdminPanelController : Controller
    {
        [NonAction]
        public IActionResult SynonymsForStrings()
        {
            HttpContext.Items["pageID"] = AdminPanelPages.SynonymsForStrings;
            return View("SynonymsForStrings", db.SynonymsForStrings.AsNoTracking().ToArray());
        }
    }
}