using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Treynessen.Database.Entities;

namespace Treynessen.Controllers
{
    public partial class PageController : Controller
    {
        [NonAction]
        public IActionResult ProductPage(ProductPage productPage)
        {
            db.Entry(productPage).State = EntityState.Detached;
            return Content("It's a product page");
        }
    }
}