using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Treynessen.AdminPanelTypes;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;

namespace Treynessen.Functions
{
    public static partial class ActionsWithDatabase
    {
        public static bool EditPage(CMSDatabase db, AdminPanelModel model, HttpContext context)
        {
            if (!model.itemID.HasValue || !model.PageType.HasValue || model.PageModel == null)
                return false;
            switch (model.PageType)
            {
                case PageType.Usual:
                    UsualPage usualPage = db.UsualPages.FirstOrDefaultAsync(up => up.ID == model.itemID).Result;
                    if (usualPage == null)
                        return false;
                    db.Entry(usualPage).State = EntityState.Detached;
                    break;
                case PageType.Category:
                    CategoryPage categoryPage = db.CategoryPages.FirstOrDefaultAsync(cp => cp.ID == model.itemID).Result;
                    if (categoryPage == null)
                        return false;
                    db.Entry(categoryPage).State = EntityState.Detached;
                    break;
                case PageType.Product:
                    ProductPage productPage = db.ProductPages.FirstOrDefaultAsync(pp => pp.ID == model.itemID).Result;
                    if (productPage == null)
                        return false;
                    db.Entry(productPage).State = EntityState.Detached;
                    break;
            }
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
