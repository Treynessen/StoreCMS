using System;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Treynessen.PagesManagement;
using Treynessen.AdminPanelTypes;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;

namespace Treynessen.Database
{
    public static partial class DatabaseInteraction
    {
        public static void EditPage(CMSDatabase db, Model model, HttpContext context, out bool successfullyCompleted)
        {
            if (!model.itemID.HasValue || model.PageModel == null || !model.PageType.HasValue)
            {
                successfullyCompleted = false;
                return;
            }
            model.PageModel.PageType = model.PageType;
            bool isMainPage = false;
            model.PageModel.IsMainPage = false;
            switch (model.PageModel.PageType)
            {
                case PageType.Usual:
                    UsualPage usualPage = db.UsualPages.FirstOrDefaultAsync(up => up.ID == model.itemID).Result;
                    db.Entry(usualPage).State = EntityState.Detached;
                    if (usualPage == null)
                    {
                        successfullyCompleted = false;
                        return;
                    }
                    model.PageModel.ID = usualPage.ID;
                    isMainPage = usualPage.RequestPath.Equals("/", StringComparison.InvariantCulture);
                    break;
                case PageType.Category:
                    CategoryPage categoryPage = db.CategoryPages.FirstOrDefaultAsync(cp => cp.ID == model.itemID).Result;
                    db.Entry(categoryPage).State = EntityState.Detached;
                    if (categoryPage == null)
                    {
                        successfullyCompleted = false;
                        return;
                    }
                    model.PageModel.ID = categoryPage.ID;
                    break;
                default:
                    successfullyCompleted = false;
                    return;
            }
            // Для блокировки выбора страницы-родителя в представлении
            context.Items["isMainPage"] = model.PageModel.IsMainPage;
            model.PageModel.PageType = model.PageModel.PageType.Value;
            Page page = PagesManagementFunctions.PageModelToPage(db, model.PageModel, context);
            if (page != null)
            {
                if (page is UsualPage up)
                {
                    if (isMainPage)
                    {
                        up.Alias = "index";
                        up.PreviousPage = null;
                    }
                    // Если родителем страницы является сама страница, то возвращаем сообщение об ошибке
                    if (up.PreviousPageID.HasValue && up.PreviousPage.ID == up.ID)
                    {
                        successfullyCompleted = false;
                        return;
                    }
                }
            }
            else
            {
                successfullyCompleted = false;
                return;
            }
            db.Update(page);

            RefreshPageAndDependencies(db, page);
            db.SaveChanges();
            successfullyCompleted = true;
        }
    }
}