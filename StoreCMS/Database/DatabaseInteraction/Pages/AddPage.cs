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
        public static void AddPage(CMSDatabase db, PageModel model, HttpContext context, out bool successfullyCompleted)
        {
            Page page = PagesManagementFunctions.PageModelToPage(db, model, context);
            if (page == null)
            {
                successfullyCompleted = false;
                return;
            }
            db.Add(page);
            db.SaveChanges();
            model.ID = page.ID;
            successfullyCompleted = true;

            LogManagementFunctions.AddAdminPanelLog(
                db: db,
                context: context,
                info: $"{page.PageName} (ID-{page.ID.ToString()}): " +
                (page is UsualPage ? (context.Items["LogLocalization"] as IAdminPanelLogLocalization)?.PageAdded
                : (context.Items["LogLocalization"] as IAdminPanelLogLocalization)?.CategoryAdded)
            );
        }
    }
}