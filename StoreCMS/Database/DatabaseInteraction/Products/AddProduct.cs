using Microsoft.AspNetCore.Http;
using Treynessen.AdminPanelTypes;
using Treynessen.PagesManagement;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;

namespace Treynessen.Database
{
    public static partial class DatabaseInteraction
    {
        public static void AddProduct(CMSDatabase db, PageModel model, int? previousPageID, HttpContext context, out bool successfullyCompleted)
        {
            if(model == null)
            {
                successfullyCompleted = false;
                return;
            }
            model.PageType = PageType.Product;
            model.PreviousPageID = previousPageID;
            ProductPage productPage = PagesManagementFunctions.PageModelToPage(db, model, context) as ProductPage;
            if (productPage == null)
            {
                successfullyCompleted = false;
                return;
            }
            productPage.ID = GetDatabaseRawID(db.ProductPages);
            db.ProductPages.Add(productPage);
            db.SaveChanges();
            successfullyCompleted = true;
        }
    }
}