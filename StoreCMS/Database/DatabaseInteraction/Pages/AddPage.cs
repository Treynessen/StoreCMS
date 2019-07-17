using Microsoft.AspNetCore.Http;
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
            switch (page)
            {
                case UsualPage up:
                    up.ID = GetDatabaseRawID(db.UsualPages);
                    break;
                case CategoryPage cp:
                    cp.ID = GetDatabaseRawID(db.CategoryPages);
                    break;
                default:
                    successfullyCompleted = false;
                    return;
            }
            db.Add(page);
            db.SaveChanges();
            model.ID = page.ID;
            successfullyCompleted = true;
        }
    }
}