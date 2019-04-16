using Microsoft.AspNetCore.Mvc;

namespace Treynessen.Controllers
{
    public partial class PagesHandlerController : Controller
    {
        [HttpGet]
        public IActionResult ProductPage()
        {
            return Content("It's a product page");
        }
    }
}