using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Treynessen.Database.Entities;

namespace Treynessen.Controllers
{
    public partial class PagesHandlerController : Controller
    {
        [HttpGet]
        public IActionResult UsualPage()
        {
            UsualPage page = HttpContext.Items["PageObject"] as UsualPage;
            db.Entry(page).Reference(up => up.Template).Load();
            if (page.Template != null)
                    return View(page.Template.TemplatePath, page);
            return Content(page.Content);
        }
    }
}