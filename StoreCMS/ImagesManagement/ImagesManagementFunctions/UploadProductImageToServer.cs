using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using Treynessen.Functions;
using Treynessen.Extensions;
using Treynessen.Localization;
using Treynessen.LogManagement;
using Treynessen.Database.Context;
using Treynessen.FileManagerManagement;

namespace Treynessen.ImagesManagement
{
    public static partial class ImagesManagementFunctions
    {
        public static void UploadProductImageToServer(CMSDatabase db, IFormFile file, int? itemID, HttpContext context, out bool successfullyUploaded)
        {
            successfullyUploaded = false;
            if (!itemID.HasValue || file == null) return;
            var env = context.RequestServices.GetRequiredService<IHostingEnvironment>();
            if (!db.ProductPages.AsNoTracking().Any(p => p.ID == itemID.Value)) return;
            string folder = $"{env.GetProductsImagesFolderFullPath()}{itemID.Value}/";
            string staging = Path.Combine(env.ContentRootPath, "App_Data", "image-staging");
            Directory.CreateDirectory(staging);
            string temporary = Path.Combine(staging, Guid.NewGuid().ToString("N") + ".tmp");
            string prepared = Path.Combine(folder, ".upload-" + Guid.NewGuid().ToString("N") + ".tmp");
            try
            {
                using (Stream input = file.OpenReadStream())
                using (Image<Rgba32> source = SixLabors.ImageSharp.Image.Load(input))
                {
                    using (var output = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                    {
                        source.Save(output, new JpegEncoder());
                        output.Flush(true);
                    }
                    Directory.CreateDirectory(folder);
                    // [Комментарий ИИ-агента]: Промежуточное имя .tmp не является оригиналом; последний Move остаётся внутри одного каталога даже при отдельном томе Storage.
                    File.Move(temporary, prepared);
                    using (ProductImageOperations.Acquire(folder))
                    {
                        var product = db.ProductPages.AsNoTracking().FirstOrDefault(p => p.ID == itemID.Value);
                        if (product == null) return;
                        if (string.IsNullOrEmpty(product.Alias) || product.Alias.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0
                            || product.Alias.Contains('/') || product.Alias.Contains('\\')) return;
                        Directory.CreateDirectory(folder);
                        string name = FileManagerManagementFunctions.GetUniqueFileOrFolderName(folder, product.Alias, ".jpg");
                        string destination = Path.Combine(folder, name);
                        // [Комментарий ИИ-агента]: Move публикует законченный файл и отказывает при существующей цели; метаданные сохраняются после него.
                        ProductImageOperations.Invalidate(folder);
                        File.Move(prepared, destination);
                        DeleteDependentImages(folder, name);
                        string shortPath = destination.Replace('\\', '/').Substring(env.GetStorageFolderFullPath().Length).Insert(0, "/");
                        var image = db.Images.FirstOrDefault(i => i.ShortPathHash == OtherFunctions.GetHashFromString(shortPath)
                            && i.ShortPath.Equals(shortPath, StringComparison.Ordinal));
                        if (image == null)
                        {
                            image = new Database.Entities.Image { ShortPath = shortPath, ShortPathHash = OtherFunctions.GetHashFromString(shortPath), FullName = name };
                            db.Images.Add(image);
                        }
                        image.Width = (uint)source.Width;
                        image.Height = (uint)source.Height;
                        // [Комментарий ИИ-агента]: Исключение SaveChanges не доказывает откат: commit мог состояться. Опубликованный оригинал сохраняем даже при ошибке; успех подтверждаем только после возврата.
                        db.SaveChanges();
                        LogManagementFunctions.AddAdminPanelLog(db, context,
                            $"{product.PageName} (ID-{product.ID}): {(context.Items["LogLocalization"] as IAdminPanelLogLocalization)?.ProductImageUploaded}");
                        successfullyUploaded = true;
                    }
                }
            }
            catch (NotSupportedException) { }
            finally
            {
                if (File.Exists(temporary)) File.Delete(temporary);
                if (File.Exists(prepared)) File.Delete(prepared);
            }
        }
    }
}
