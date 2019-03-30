using Trane.Db.Entities;
using Trane.Db.Context;
using Microsoft.AspNetCore.Mvc;

namespace Trane.Controllers
{
    public class PageController : Controller
    {
        private CMSContext db;

        public PageController(CMSContext db)
        {
            this.db = db;
        }

        public IActionResult PageHandler()
        {
            return Content("Page");
        }
    }
}
