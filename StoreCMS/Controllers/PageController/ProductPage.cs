using Microsoft.AspNetCore.Mvc;
using Treynessen.Database.Entities;

namespace Treynessen.Controllers
{
    public partial class PageController : Controller
    {
        [NonAction]
        public IActionResult ProductPage(ProductPage productPage)
        {
            db.Entry(productPage).Reference(up => up.Template).LoadAsync().Wait();
            if (productPage.Template != null)
            {
                return View(productPage.Template.TemplatePath, productPage);
            }
            return Content(productPage.Content);
        }
    }
}