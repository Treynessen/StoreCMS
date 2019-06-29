using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Treynessen.AdminPanelTypes;

namespace Treynessen.Controllers
{
    public partial class AdminPanelController : Controller
    {
        [NonAction]
        public IActionResult Chunks()
        {
            HttpContext.Items["pageID"] = AdminPanelPages.Chunks;
            return View("Templates/ChunksPage", db.Chunks.AsNoTracking().ToArray());
        }
    }
}