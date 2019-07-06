using Microsoft.AspNetCore.Mvc;
using Treynessen.Database.Entities;

namespace Treynessen.Controllers
{
    public partial class PageController : Controller
    {
        [NonAction]
        public IActionResult UsualPage(UsualPage usualPage)
        {
            db.Entry(usualPage).Reference(up => up.Template).LoadAsync().Wait();
            if (usualPage.Template != null)
            {
                return View(usualPage.Template.TemplatePath, usualPage);
            }
            return Content(usualPage.Content);
        }
    }
}