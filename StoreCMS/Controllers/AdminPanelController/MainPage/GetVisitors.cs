using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Treynessen.Controllers
{
    public partial class AdminPanelController : Controller
    {
        [NonAction]
        public IActionResult GetVisitors()
        {
            return View("MainPage/GetVisitors", db.Visitors.AsNoTracking().ToArray());
        }
    }
}