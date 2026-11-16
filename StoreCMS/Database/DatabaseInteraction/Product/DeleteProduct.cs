using System;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Treynessen.Functions;
using Treynessen.Extensions;
using Treynessen.Localization;
using Treynessen.LogManagement;
using Treynessen.ImagesManagement;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;

namespace Treynessen.Database
{
    public static partial class DatabaseInteraction
    {
        public static void DeleteProduct(CMSDatabase db, int? productID, HttpContext context, out bool successfullyDeleted)
        {
            if (!productID.HasValue)
            {
                successfullyDeleted = false;
                return;
            }
            ProductPage product = db.ProductPages.FirstOrDefault(pp => pp.ID == productID.Value);
            if (product == null)
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
                    files.Stage(product.ID);
                    string prefix = $"/images/products/{product.ID}/";
                    db.Images.RemoveRange(db.Images.Where(image => image.ShortPath.StartsWith(prefix)));
                    db.Entry(product).Reference(pp => pp.PreviousPage).Load();
                    --product.PreviousPage.ProductsCount;
                    db.ProductPages.Remove(product);
                    db.SaveChanges();
                    commitAttempted = true;
                    transaction.Commit();
                }
                catch (Exception failure)
                {
                    // [Комментарий ИИ-агента]: Файлы возвращаем только после подтверждённого rollback, не после неизвестного COMMIT.
                    if (commitAttempted) throw;
                    try { transaction.Rollback(); }
                    catch (Exception rollback) { throw new AggregateException("Не подтверждён откат удаления товара; требуется сверка БД и App_Data/product-deletions.", failure, rollback); }
                    files.Restore(failure);
                    throw;
                }
                files.DeleteCommitted();
            }
            successfullyDeleted = true;

            LogManagementFunctions.AddAdminPanelLog(
                db: db,
                context: context,
                info: $"{product.PageName} (ID-{product.ID.ToString()}): {(context.Items["LogLocalization"] as IAdminPanelLogLocalization)?.ProductDeleted}"
            );
        }
    }
}
