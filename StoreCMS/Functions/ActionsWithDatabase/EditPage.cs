using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Treynessen.AdminPanelTypes;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;

namespace Treynessen.Functions
{
    public static partial class ActionsWithDatabase
    {
        public static bool EditPage(CMSDatabase db, AdminPanelModel model, HttpContext context)
        {
            if (!model.itemID.HasValue || !model.PageType.HasValue)
                return false;
            model.PageModel.PageType = model.PageType.Value;
            Page page = OtherFunctions.PageModelToPage(db, model.PageModel, context);
            if (page != null)
            {
                page.ID = model.itemID.Value;
                if (page is UsualPage up)
                {
                    if (up.PreviousPageID.HasValue && up.PreviousPage.ID == up.ID)
                        return false;
                }
            }
            if (!Validator.TryValidateObject(page, new ValidationContext(page), null))
                return false;
            OtherFunctions.SetUniqueAliasName(db, page);
            db.Update(page);
            RefreshPageAndDependencies(db, page);
            db.SaveChanges();
            return true;
        }
    }
}
