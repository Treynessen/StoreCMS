using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Treynessen.Database.Entities;

namespace Treynessen.Controllers
{
    public partial class AdminPanelController : Controller
    {
        [NonAction]
        public IActionResult Categories()
        {
            var categories = db.CategoryPages.OrderBy(cp => cp.PageName).ToArrayAsync().Result;
            return View("Products/Categories", categories);
        } 
    }
}
