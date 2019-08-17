using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Treynessen.Functions;
using Treynessen.Localization;
using Treynessen.LogManagement;
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
            Page editablePage = null;
            switch (model.PageModel.PageType)
            {
                case PageType.Usual:
                    editablePage = db.UsualPages.AsNoTracking().FirstOrDefault(up => up.ID == model.itemID);
                    if (editablePage == null)
                    {
                        successfullyCompleted = false;
                        return;
                    }
                    model.PageModel.ID = editablePage.ID;
                    isMainPage = editablePage.RequestPath.Equals("/", StringComparison.Ordinal);
                    break;
                case PageType.Category:
                    editablePage = db.CategoryPages.AsNoTracking().FirstOrDefault(cp => cp.ID == model.itemID);
                    if (editablePage == null)
                    {
                        successfullyCompleted = false;
                        return;
                    }
                    model.PageModel.ID = editablePage.ID;
                    break;
                default:
                    successfullyCompleted = false;
                    return;
            }
            model.PageModel.PageType = model.PageModel.PageType.Value;
            Page editedPage = PagesManagementFunctions.PageModelToPage(db, model.PageModel, context);
            if (editedPage != null)
            {
                if (editedPage is UsualPage up)
                {
                    if (isMainPage)
                    {
                        up.Alias = "index";
                        up.RequestPath = "/";
                        up.RequestPathHash = OtherFunctions.GetHashFromString(up.RequestPath);
                        up.PreviousPage = null;
                    }
                    // Если родителем страницы является сама страница или зависимая страница, то возвращаем сообщение об ошибке
                    if (up.PreviousPage != null && PagesManagementFunctions.GetDependentPageIDs(db, up).Contains(up.PreviousPage.ID))
                    {
                        successfullyCompleted = false;
                        return;
                    }
                }
                else if (editedPage is CategoryPage cp)
                {
                    cp.ProductsCount = (editablePage as CategoryPage).ProductsCount;
                    cp.LastProductTemplateID = (editablePage as CategoryPage).LastProductTemplateID;
                }
            }
            else
            {
                successfullyCompleted = false;
                return;
            }
            db.Update(editedPage);

            // Обновляем все зависимые страницы, если изменилось имя страницы и/или url страницы
            if (!editablePage.PageName.Equals(editedPage.PageName, StringComparison.InvariantCulture)
                || !editablePage.Alias.Equals(editedPage.RequestPath, StringComparison.Ordinal))
            {
                if (editedPage is UsualPage)
                {
                    List<UsualPage> usualPages = db.UsualPages.Where(p => p.PreviousPageID == editedPage.ID).ToList();
                    List<CategoryPage> categoryPages = db.CategoryPages.Where(p => p.PreviousPageID == editedPage.ID).ToList();
                    foreach (var u_page in usualPages)
                        RefreshPageAndDependencies(db, u_page);
                    foreach (var c_page in categoryPages)
                        RefreshPageAndDependencies(db, c_page);
                }
                if (editedPage is CategoryPage)
                {
                    List<ProductPage> productPages = db.ProductPages.Where(p => p.PreviousPageID == editedPage.ID).ToList();
                    foreach (var p_page in productPages)
                        RefreshPageAndDependencies(db, p_page);
                }
            }
            db.SaveChanges();
            successfullyCompleted = true;

            LogManagementFunctions.AddAdminPanelLog(
                db: db, 
                context: context,
                info: $"{editablePage.PageName} (ID-{editablePage.ID.ToString()}): " +
                (editablePage is UsualPage ? (context.Items["LogLocalization"] as IAdminPanelLogLocalization)?.PageEdited
                : (context.Items["LogLocalization"] as IAdminPanelLogLocalization)?.CategoryEdited)
            );
        }
    }
}