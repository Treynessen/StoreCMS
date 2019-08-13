using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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
        public static void RenameImageAndDependencies(CMSDatabase db, IHostingEnvironment env, string pathToImages,
            string oldImageName, string newImageName, string imageExtension, bool saveChangesInDB = true)
        {
            if (string.IsNullOrEmpty(pathToImages) || string.IsNullOrEmpty(oldImageName)
                || string.IsNullOrEmpty(newImageName) || !Directory.Exists(pathToImages))
                return;
            if (!pathToImages[pathToImages.Length - 1].Equals('/'))
                pathToImages = pathToImages.Insert(pathToImages.Length, "/");
            string[] images = Directory.GetFiles(pathToImages, $"*{oldImageName}*{imageExtension}");
            Regex regex = new Regex($"{oldImageName}(_\\d+x\\d+)?(_q\\d{{1,3}})?{imageExtension}");
            images = (from img in images
                      where regex.IsMatch(img)
                      select img).ToArray();
            foreach (var img in images)
            {
                string fileEnding = img.Substring(pathToImages.Length + oldImageName.Length);
                File.Move(img, $"{pathToImages}{newImageName}{fileEnding}");
            }
            string shortPathToOldImage = pathToImages.Replace(env.GetStorageFolderFullPath(), string.Empty).Insert(0, "/") + oldImageName + imageExtension;
            string shortPathToNewImage = pathToImages.Replace(env.GetStorageFolderFullPath(), string.Empty).Insert(0, "/") + newImageName + imageExtension;
            // Изменяем данные в БД
            // Если в БД есть неудаленная информация, то удаляем её
            Image image = db.Images.FirstOrDefault(img => img.ShortPathHash == OtherFunctions.GetHashFromString(shortPathToNewImage) && img.ShortPath.Equals(shortPathToNewImage, StringComparison.Ordinal));
            if (image != null && db.Entry(image).State != EntityState.Modified)
                db.Remove(image);
            image = db.Images.FirstOrDefault(img => img.ShortPathHash == OtherFunctions.GetHashFromString(shortPathToOldImage) && img.ShortPath.Equals(shortPathToOldImage, StringComparison.Ordinal));
            if (image != null)
            {
                if (db.Entry(image).State == EntityState.Deleted)
                    db.Entry(image).State = EntityState.Modified;
                image.ShortPath = shortPathToNewImage;
                image.ShortPathHash = OtherFunctions.GetHashFromString(shortPathToNewImage);
                image.FullName = shortPathToNewImage.Substring(shortPathToNewImage.LastIndexOf('/') + 1);
            }
            if (saveChangesInDB)
                db.SaveChanges();
        }
    }
}