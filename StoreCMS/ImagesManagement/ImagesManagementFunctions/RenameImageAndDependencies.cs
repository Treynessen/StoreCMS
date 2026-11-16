using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Treynessen.Functions;
using Treynessen.Extensions;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;

namespace Treynessen.ImagesManagement
{
    public static partial class ImagesManagementFunctions
    {
        public static void RenameImageAndDependencies(CMSDatabase db, IWebHostEnvironment env, string pathToImages,
            string oldImageName, string newImageName, string imageExtension, bool saveChangesInDB = true)
            => RenameImageAndDependenciesCore(db, env, pathToImages, oldImageName, newImageName, imageExtension, saveChangesInDB, File.Move);

        internal static void RenameImageAndDependenciesCore(CMSDatabase db, IWebHostEnvironment env, string pathToImages,
            string oldImageName, string newImageName, string imageExtension, bool saveChangesInDB, Action<string, string> move)
        {
            if (string.IsNullOrEmpty(pathToImages) || string.IsNullOrEmpty(oldImageName)
                || string.IsNullOrEmpty(newImageName) || !Directory.Exists(pathToImages))
                return;
            pathToImages = Path.GetFullPath(pathToImages).Replace('\\', '/');
            using (ProductImageOperations.Acquire(pathToImages))
            {
                if (!pathToImages[pathToImages.Length - 1].Equals('/'))
                    pathToImages = pathToImages.Insert(pathToImages.Length, "/");
                string[] images = Directory.GetFiles(pathToImages);
                Regex regex = new Regex($"\\A{Regex.Escape(oldImageName)}(_\\d+x\\d+)?(_q\\d{{1,3}})?{Regex.Escape(imageExtension)}\\z");
                images = (from img in images
                          where regex.IsMatch(Path.GetFileName(img))
                          select img).OrderBy(img => Path.GetFileName(img) == oldImageName + imageExtension ? 0 : 1)
                              .ThenBy(img => img, StringComparer.Ordinal).ToArray();
                // [Комментарий ИИ-агента]: Известную коллизию всего семейства отклоняем до первого Move; при повторе после SQL-ошибки допускаем уже перенесённое семейство.
                if (images.Length == 0 && !File.Exists(Path.Combine(pathToImages, newImageName + imageExtension)))
                    throw new FileNotFoundException("Не найден ни исходный, ни целевой оригинал изображения.");
                foreach (var img in images)
                {
                    string target = Path.Combine(pathToImages, newImageName + Path.GetFileName(img).Substring(oldImageName.Length));
                    if (File.Exists(target) || Directory.Exists(target)) throw new IOException("Целевое имя изображения уже занято.");
                }
                if (images.Length > 0) ProductImageOperations.Invalidate(pathToImages);
                var moved = new List<KeyValuePair<string, string>>();
                try
                {
                    foreach (var img in images)
                    {
                        string target = Path.Combine(pathToImages, newImageName + Path.GetFileName(img).Substring(oldImageName.Length));
                        move(img, target);
                        moved.Add(new KeyValuePair<string, string>(img, target));
                    }
                }
                catch (Exception failure) when (failure is IOException || failure is UnauthorizedAccessException)
                {
                    var errors = new List<Exception> { failure };
                    // [Комментарий ИИ-агента]: Компенсируем только известный файловый отказ до SQL; существующие исходные имена не перезаписываем.
                    for (int i = moved.Count - 1; i >= 0; i--)
                    {
                        try { move(moved[i].Value, moved[i].Key); }
                        catch (Exception error) when (error is IOException || error is UnauthorizedAccessException) { errors.Add(error); }
                    }
                    if (errors.Count > 1) throw new AggregateException("Не удалось полностью откатить переименование; требуется адресная сверка семейства.", errors);
                    throw;
                }
                string shortPathToOldImage = pathToImages.Substring(env.GetStorageFolderFullPath().Length).Insert(0, "/") + oldImageName + imageExtension;
                string shortPathToNewImage = pathToImages.Substring(env.GetStorageFolderFullPath().Length).Insert(0, "/") + newImageName + imageExtension;
                Image image = db.Images.FirstOrDefault(img => img.ShortPathHash == OtherFunctions.GetHashFromString(shortPathToOldImage) && img.ShortPath == shortPathToOldImage);
                if (image != null)
                {
                    Image target = db.Images.FirstOrDefault(img => img.ShortPathHash == OtherFunctions.GetHashFromString(shortPathToNewImage) && img.ShortPath == shortPathToNewImage);
                    if (target != null && !ReferenceEquals(image, target) && db.Entry(target).State != EntityState.Modified) db.Remove(target);
                    if (db.Entry(image).State == EntityState.Deleted)
                        db.Entry(image).State = EntityState.Modified;
                    image.ShortPath = shortPathToNewImage;
                    image.ShortPathHash = OtherFunctions.GetHashFromString(shortPathToNewImage);
                    image.FullName = shortPathToNewImage.Substring(shortPathToNewImage.LastIndexOf('/') + 1);
                }
                if (saveChangesInDB)
                    // [Комментарий ИИ-агента]: SQL-ошибка может означать неизвестный исход COMMIT; файлы здесь не откатываем и COMMIT вслепую не повторяем.
                    db.SaveChanges();
            }
        }
    }
}
