using Microsoft.AspNetCore.Mvc;
using Treynessen.Database.Entities;

namespace Treynessen.Controllers
{
    public partial class PagesHandlerController : Controller
    {
        [NonAction]
        public IActionResult CategoryPage(CategoryPage categoryPage)
        {
            db.Entry(categoryPage).Reference(cp => cp.Template).Load();
            db.Entry(categoryPage).Collection(cp => cp.ProductPages).Load();
            if (categoryPage.Template != null)
                return View(categoryPage.Template.TemplatePath, categoryPage);
            return Content(categoryPage.Content);
        }
    }
}