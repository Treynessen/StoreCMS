using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Treynessen.Controllers
{
    public partial class AdminPanelController : Controller
    {
        [NonAction]
        public IActionResult Chunks()
        {
            return View("Templates/ChunksPage", db.Chunks.AsNoTracking().ToArray());
        }
    }
}