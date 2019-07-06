using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Treynessen.Database.Entities;

namespace Treynessen.Controllers
{
    public partial class PageController : Controller
    {
        [NonAction]
        public IActionResult CategoryPage(CategoryPage categoryPage)
        {
            db.Entry(categoryPage).Reference(cp => cp.Template).LoadAsync().Wait();
            db.Entry(categoryPage).Collection(cp => cp.ProductPages).LoadAsync().Wait();
            List<ProductPage> products = categoryPage.ProductPages;
            HttpContext.Items["products"] = products;
            if (categoryPage.Template != null)
            {
                return View(categoryPage.Template.TemplatePath, categoryPage);
            }
            return Content(categoryPage.Content);
        }
    }
}