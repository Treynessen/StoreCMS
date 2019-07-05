using Microsoft.AspNetCore.Mvc;
using Treynessen.Database.Entities;

namespace Treynessen.Controllers
{
    public partial class PageController : Controller
    {
        [NonAction]
        public IActionResult ProductPage(ProductPage productPage)
        {
            return Content("It's a product page");
        }
    }
}