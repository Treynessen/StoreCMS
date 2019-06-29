using Microsoft.AspNetCore.Mvc;
using Treynessen.AdminPanelTypes;

namespace Treynessen.Controllers
{
    public partial class AdminPanelController : Controller
    {
        [NonAction]
        public IActionResult AddPage(PageModel model = null)
        {
            HttpContext.Items["pageID"] = AdminPanelPages.AddPage;
            return View("Pages/AddPage", model);
        }
    }
}