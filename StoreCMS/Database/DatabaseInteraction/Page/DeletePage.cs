using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Treynessen.Functions;
using Treynessen.Extensions;
using Treynessen.Localization;
using Treynessen.LogManagement;
using Treynessen.PagesManagement;
using Treynessen.ImagesManagement;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;

namespace Treynessen.Database
{
    public static partial class DatabaseInteraction
    {
        public static void DeletePage(CMSDatabase db, PageType? pageType, int? itemID, HttpContext context, out bool successfullyDeleted)
        {
            if (!pageType.HasValue || !itemID.HasValue)
            {
                successfullyDeleted = false;
                return;
            }
            Page page = null;
            switch (pageType)
            {
                case PageType.Usual:
                    page = db.UsualPages.FirstOrDefault(p => p.ID == itemID);
                    break;
                case PageType.Category:
                    page = db.CategoryPages.FirstOrDefault(p => p.ID == itemID);
                    break;
                default:
                    successfullyDeleted = false;
                    return;
            }
            if (page == null)
            {
                successfullyDeleted = false;
                return;
            }
            IWebHostEnvironment env = context.RequestServices.GetRequiredService<IWebHostEnvironment>();
            using (var files = new ProductDeletionFiles(env))
            using (var transaction = db.Database.BeginTransaction())
            {
                bool commitAttempted = false;
                try
                {
                    if (page is UsualPage up)
                    {
                        // Получаем все зависимые страницы
                        List<UsualPage> usualPages = db.UsualPages.Where(p => p.PreviousPageID == up.ID).ToList();
                        List<CategoryPage> categoryPages = db.CategoryPages.Where(p => p.PreviousPageID == up.ID).ToList();
                        db.UsualPages.Remove(up);
                        db.SaveChanges();
                        // Обновляем полученные зависимые страницы
                        foreach (var u_page in usualPages)
                        {
                            u_page.PreviousPageID = up.PreviousPageID;
                            RefreshPageAndDependencies(db, u_page);
                        }
                        foreach (var c_page in categoryPages)
                        {
                            c_page.PreviousPageID = up.PreviousPageID;
                            RefreshPageAndDependencies(db, c_page);
                        }
                    }
                    else if (page is CategoryPage cp)
                    {
                        cp.ProductPages = db.ProductPages.Where(pp => pp.PreviousPageID == cp.ID).ToList();
                        foreach (var p in cp.ProductPages.OrderBy(p => p.ID))
                        {
                            files.Stage(p.ID);
                            string prefix = $"/images/products/{p.ID}/";
                            db.Images.RemoveRange(db.Images.Where(image => image.ShortPath.StartsWith(prefix)));
                        }
                        db.Remove(page);
                    }
                    db.SaveChanges();
                    commitAttempted = true;
                    transaction.Commit();
                }
                catch (Exception failure)
                {
                    // [Комментарий ИИ-агента]: Все SQL-изменения страницы откатываются вместе; каталоги восстанавливаются после подтверждённого rollback.
                    if (commitAttempted) throw;
                    try { transaction.Rollback(); }
                    catch (Exception rollback) { throw new AggregateException("Не подтверждён откат удаления страницы; требуется сверка БД и App_Data/product-deletions.", failure, rollback); }
                    files.Restore(failure);
                    throw;
                }
                files.DeleteCommitted();
            }
            successfullyDeleted = true;

            LogManagementFunctions.AddAdminPanelLog(
                db: db,
                context: context,
                info: $"{page.PageName} (ID-{page.ID.ToString()}): "+
                (page is UsualPage ? (context.Items["LogLocalization"] as IAdminPanelLogLocalization)?.PageDeleted
                : (context.Items["LogLocalization"] as IAdminPanelLogLocalization)?.CategoryDeleted)
            );
        }
    }
}
