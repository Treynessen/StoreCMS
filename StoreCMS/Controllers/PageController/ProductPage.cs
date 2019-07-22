using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Treynessen.Database.Entities;

namespace Treynessen.Controllers
{
    public partial class PageController : Controller
    {
        [NonAction]
        public IActionResult ProductPage(ProductPage productPage)
        {
            Template template = db.Templates.AsNoTracking().FirstOrDefault(t => t.ID == productPage.TemplateId);
            if (template != null)
            {
                return View(template.TemplatePath, productPage);
            }
            return Content(productPage.Content);
        }
    }
}