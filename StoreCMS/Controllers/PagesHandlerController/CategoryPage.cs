using Microsoft.AspNetCore.Mvc;
using Treynessen.Database.Entities;

namespace Treynessen.Controllers
{
    public partial class PagesHandlerController : Controller
    {
        [NonAction]
        public IActionResult CategoryPage(CategoryPage categoryPage)
        {
            return Content("It's a category page");
        }
    }
}