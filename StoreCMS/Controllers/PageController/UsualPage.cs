using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Treynessen.Database.Entities;

namespace Treynessen.Controllers
{
    public partial class PageController : Controller
    {
        [NonAction]
        public IActionResult UsualPage(UsualPage usualPage)
        {
            Template template = db.Templates.AsNoTracking().FirstOrDefault(t => t.ID == usualPage.TemplateId);
            if (template != null)
            {
                return View(template.TemplatePath, usualPage);
            }
            return Content(usualPage.Content);
        }
    }
}