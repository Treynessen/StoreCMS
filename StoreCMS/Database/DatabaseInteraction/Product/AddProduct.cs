using Microsoft.AspNetCore.Http;
using Treynessen.Localization;
using Treynessen.LogManagement;
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
            ++productPage.PreviousPage.ProductsCount;
            productPage.PreviousPage.LastProductTemplate = productPage.Template;
            db.ProductPages.Add(productPage);
            db.SaveChanges();
            model.ID = productPage.ID;
            successfullyCompleted = true;

            LogManagementFunctions.AddAdminPanelLog(
                db: db,
                context: context,
                info: $"{productPage.PageName} (ID-{productPage.ID.ToString()}): {(context.Items["LogLocalization"] as IAdminPanelLogLocalization)?.ProductAdded}"
            );
        }
    }
}