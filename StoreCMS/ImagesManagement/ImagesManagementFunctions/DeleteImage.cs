using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Treynessen.Functions;
using Treynessen.Extensions;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;

namespace Treynessen.ImagesManagement
{
    public static partial class ImagesManagementFunctions
    {
        public static void DeleteImage(string pathToImageFolder, string imageFullName, CMSDatabase db, IHostingEnvironment env)
        {
            if (string.IsNullOrEmpty(pathToImageFolder) || string.IsNullOrEmpty(imageFullName))
                return;
            if (!pathToImageFolder[pathToImageFolder.Length - 1].Equals('/'))
                pathToImageFolder = pathToImageFolder.Insert(pathToImageFolder.Length, "/");
            if (!Directory.Exists(pathToImageFolder))
                return;
            string pathToFile = $"{pathToImageFolder}{imageFullName}";
            if (!File.Exists(pathToFile))
                return;
            File.Delete(pathToFile);
            // Удаляем информацию об изображении из БД
            string shortPathToImage = pathToImageFolder.Substring(env.GetStorageFolderFullPath().Length).Insert(0, "/") + imageFullName;
            Image image = db.Images.FirstOrDefault(img => img.ShortPathHash == OtherFunctions.GetHashFromString(shortPathToImage) && img.ShortPath.Equals(shortPathToImage, StringComparison.Ordinal));
            if (image != null)
            {
                db.Images.Remove(image);
                db.SaveChanges();
            }
            // Если у изображения были зависимости (сжатые или ресайзнутые вариации), то удаляем их
            DeleteDependentImages(pathToImageFolder, imageFullName);
        }
    }
}