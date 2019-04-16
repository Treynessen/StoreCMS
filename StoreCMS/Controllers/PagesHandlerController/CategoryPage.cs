using Microsoft.AspNetCore.Mvc;

namespace Treynessen.Controllers
{
    public partial class PagesHandlerController : Controller
    {
        [HttpGet]
        public IActionResult CategoryPage()
        {
            return Content("It's a category page");
        }
    }
}