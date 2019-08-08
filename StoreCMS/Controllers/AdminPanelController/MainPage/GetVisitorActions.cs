using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Treynessen.Localization;
using Treynessen.PagesManagement;
using Treynessen.Database.Entities;

namespace Treynessen.Controllers
{
    public partial class AdminPanelController : Controller
    {
        [NonAction]
        public IActionResult GetVisitorActions(int? visitorID)
        {
            if (!visitorID.HasValue)
                return Content(string.Empty);
            Visitor visitor = db.Visitors.AsNoTracking().FirstOrDefault(v => v.ID == visitorID.Value);
            if (visitor == null)
                return Content(string.Empty);
            VisitedPage[] visitedPages = db.VisitedPages.AsNoTracking().Where(vp => vp.VisitorId == visitorID.Value).ToArray();
            IVisitorsLocalization localization = HttpContext.RequestServices.GetRequiredService<IVisitorsLocalization>();
            StringBuilder contentBuilder = new StringBuilder();
            if (visitedPages.Length > 0)
                contentBuilder.Append($"<p>{localization.ActionsOfUser} {visitor.IPAdress}:</p>");
            foreach (var visitedPage in visitedPages)
            {
                switch (visitedPage.PageType)
                {
                    case PageType.Usual:
                        UsualPage usualPage = db.UsualPages.AsNoTracking().FirstOrDefault(up => up.ID == visitedPage.VisitedPageId);
                        if (usualPage != null)
                            contentBuilder.Append($"<p>{localization.VisitedUsualPage}: <b>{usualPage.PageName} (ID-{usualPage.ID.ToString()})</b></p>\n");
                        break;
                    case PageType.Category:
                        CategoryPage categoryPage = db.CategoryPages.AsNoTracking().FirstOrDefault(cp => cp.ID == visitedPage.VisitedPageId);
                        if (categoryPage != null)
                            contentBuilder.Append($"<p>{localization.VisitedCategoryPage}: <b>{categoryPage.PageName} (ID-{categoryPage.ID.ToString()})</b></p>\n");
                        break;
                    case PageType.Product:
                        ProductPage productPage = db.ProductPages.AsNoTracking().FirstOrDefault(pp => pp.ID == visitedPage.VisitedPageId);
                        if (productPage != null)
                            contentBuilder.Append($"<p>{localization.VisitedCategoryPage}: <b>{productPage.PageName} (ID-{productPage.ID.ToString()})</b></p>\n");
                        break;
                }
            }
            return Content(contentBuilder.ToString());
        }
    }
}