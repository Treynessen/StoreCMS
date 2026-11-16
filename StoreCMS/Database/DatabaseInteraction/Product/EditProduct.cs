using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Treynessen.Extensions;
using Treynessen.Functions;
using Treynessen.Localization;
using Treynessen.LogManagement;
using Treynessen.AdminPanelTypes;
using Treynessen.PagesManagement;
using Treynessen.ImagesManagement;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;

namespace Treynessen.Database
{
    public static partial class DatabaseInteraction
    {
        public static void EditProduct(CMSDatabase db, PageModel model, int? productID, HttpContext context, out bool successfullyCompleted)
        {
            if (model == null || !productID.HasValue)
            {
                successfullyCompleted = false;
                return;
            }
            IWebHostEnvironment env = context.RequestServices.GetRequiredService<IWebHostEnvironment>();
            string folder = $"{env.GetProductsImagesFolderFullPath()}{productID.Value}/";
            using (ProductImageOperations.Acquire(folder))
            {
                model.PageType = PageType.Product;
                ProductPage editableProduct = db.ProductPages.AsNoTracking().FirstOrDefault(pp => pp.ID == productID.Value);
                if (editableProduct == null)
                {
                    successfullyCompleted = false;
                    return;
                }
                model.ID = editableProduct.ID;
                model.PreviousPageID = editableProduct.PreviousPageID;
                ProductPage editedProduct = PagesManagementFunctions.PageModelToPage(db, model, context) as ProductPage;
                if (editedProduct == null)
                {
                    successfullyCompleted = false;
                    return;
                }
                var plan = new List<(string Source, string Temporary, string Target)>();
                var moved = new List<(string Source, string Target)>();
                if (!editableProduct.Alias.Equals(editedProduct.Alias, StringComparison.Ordinal) && Directory.Exists(folder))
                {
                    // [Комментарий ИИ-агента]: Планируем все семейства до изменений; промежуточные имена исключают коллизии собственных файлов при alias -> alias_2.
                    var pattern = new Regex(@"\A" + Regex.Escape(editableProduct.Alias) + @"(?:_\d+)?(?:_\d+x\d+)?(?:_q\d{1,3})?\.jpg\z");
                    foreach (string source in Directory.GetFiles(folder).Where(p => pattern.IsMatch(Path.GetFileName(p))))
                    {
                        string target = Path.Combine(folder, editedProduct.Alias + Path.GetFileName(source).Substring(editableProduct.Alias.Length));
                        plan.Add((source, Path.Combine(folder, ".edit-" + Guid.NewGuid().ToString("N") + ".tmp"), target));
                    }
                    var sources = new HashSet<string>(plan.Select(p => Path.GetFullPath(p.Source)), StringComparer.OrdinalIgnoreCase);
                    foreach (var item in plan)
                        if (Directory.Exists(item.Target) || (File.Exists(item.Target) && !sources.Contains(Path.GetFullPath(item.Target))))
                            throw new IOException("Целевое имя изображения уже занято.");
                }
                // [Комментарий ИИ-агента]: Товар, URL, шаблон категории и Images объединены SQL-транзакцией; известный отказ до COMMIT компенсирует все выполненные Move.
                using (var transaction = db.Database.BeginTransaction())
                {
                    bool commitAttempted = false;
                    try
                    {
                        editedProduct.PreviousPage.LastProductTemplate = editedProduct.Template;
                        db.ProductPages.Update(editedProduct);
                        db.SaveChanges();
                        if (plan.Count > 0) ProductImageOperations.Invalidate(folder);
                        foreach (var item in plan)
                        {
                            File.Move(item.Source, item.Temporary);
                            moved.Add((item.Source, item.Temporary));
                        }
                        foreach (var item in plan)
                        {
                            File.Move(item.Temporary, item.Target);
                            moved.Add((item.Temporary, item.Target));
                        }
                        string ShortPath(string path) => "/" + Path.GetRelativePath(env.GetStorageFolderFullPath(), path).Replace('\\', '/');
                        var renames = plan.ToDictionary(p => ShortPath(p.Source), p => ShortPath(p.Target), StringComparer.Ordinal);
                        var rows = db.Images.Where(i => i.ShortPath.StartsWith($"/images/products/{productID.Value}/")).ToArray();
                        var changed = rows.Where(i => renames.ContainsKey(i.ShortPath)).ToArray();
                        foreach (var image in changed)
                        {
                            string target = renames[image.ShortPath];
                            if (rows.Any(i => i.ShortPath == target && !changed.Contains(i)))
                                throw new IOException("Целевые метаданные изображения уже заняты.");
                        }
                        foreach (var image in changed)
                        {
                            image.ShortPath = renames[image.ShortPath];
                            image.ShortPathHash = OtherFunctions.GetHashFromString(image.ShortPath);
                            image.FullName = Path.GetFileName(image.ShortPath);
                        }
                        db.SaveChanges();
                        commitAttempted = true;
                        transaction.Commit();
                    }
                    catch (Exception failure)
                    {
                        // [Комментарий ИИ-агента]: Неизвестный исход COMMIT требует независимой сверки; файлы не откатываем вслепую.
                        if (commitAttempted) throw;
                        var errors = new List<Exception> { failure };
                        try { transaction.Rollback(); }
                        catch (Exception rollback) { throw new AggregateException("Не подтверждён откат БД; требуется сверка товара и файлов.", failure, rollback); }
                        for (int i = moved.Count - 1; i >= 0; --i)
                        {
                            try { File.Move(moved[i].Target, moved[i].Source); }
                            catch (Exception error) when (error is IOException || error is UnauthorizedAccessException) { errors.Add(error); }
                        }
                        if (errors.Count > 1) throw new AggregateException("Не завершена компенсация EditProduct; сохраните файлы и сверьте товар и Images.", errors);
                        throw;
                    }
                }
                successfullyCompleted = true;

                LogManagementFunctions.AddAdminPanelLog(
                    db: db,
                    context: context,
                    info: $"{editableProduct.PageName} (ID-{editableProduct.ID.ToString()}): {(context.Items["LogLocalization"] as IAdminPanelLogLocalization)?.ProductEdited}"
                );
            }
        }
    }
}