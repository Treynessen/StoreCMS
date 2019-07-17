using Microsoft.AspNetCore.Mvc;
using Treynessen.AdminPanelTypes;

namespace Treynessen.Controllers
{
    public partial class AdminPanelController : Controller
    {
        [NonAction]
        public IActionResult AddChunk()
        {
            HttpContext.Items["pageID"] = AdminPanelPages.AddChunk;
            return View("Chunks/AddChunk");
        }
    }
}