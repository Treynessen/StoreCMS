using Microsoft.AspNetCore.Mvc;
using Treynessen.AdminPanelTypes;

namespace Treynessen.Controllers
{
    public partial class AdminPanelController : Controller
    {
        [NonAction]
        public IActionResult AddProduct(PageModel model = null)
        {
            SetRoutes("AddProduct");
            return View("Products/AddProduct", model);
        }
    }
}