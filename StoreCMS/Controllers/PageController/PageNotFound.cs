using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Treynessen.Database.Entities;
using Treynessen.SettingsManagement;

namespace Treynessen.Controllers
{
    public partial class PageController : Controller
    {
        [HttpGet]
        public IActionResult PageNotFound()
        {
            HttpContext.Response.StatusCode = 404;
            ConfigurationHandler config = HttpContext.RequestServices.GetRequiredService<ConfigurationHandler>();
            int? pageNotFoundTemplateId = null;
            try
            {
                pageNotFoundTemplateId = Convert.ToInt32(config.GetConfigValue("TemplateSettingsForSpecialPages:PageNotFoundTemplateId"));
                Template template = db.Templates.AsNoTracking().FirstOrDefault(t => t.ID == pageNotFoundTemplateId);
                if (template != null)
                    return View(template.TemplatePath);
                else throw new FormatException();
            }
            catch (FormatException)
            {
                return Content("Page not found");
            }
        }
    }
}