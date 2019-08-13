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
    public partial class ImageHandler
    {
        private CMSDatabase db;

        private IHostingEnvironment env;
        
        private string pathToImageFolder;
        private string sourceImageName;
        private string sourceImageExtension;
        private string sourceImageFullName;
        private string sourceImageFullPath;
        private string sourceImageShortPath;

        private bool isExisted = false;

        public ImageHandler(string pathToImage, bool itsFullPath, CMSDatabase db,  IHostingEnvironment env)
        {
            this.db = db;
            this.env = env;
            if (pathToImage[pathToImage.Length - 1].Equals('/'))
                return;
            pathToImageFolder = pathToImage.Substring(0, pathToImage.LastIndexOf('/') + 1);
            sourceImageName = pathToImage.Substring(pathToImageFolder.Length);
            if (!sourceImageName.Contains("."))
                return;
            sourceImageName = sourceImageName.Substring(0, sourceImageName.LastIndexOf('.'));
            sourceImageExtension = pathToImage.Substring(pathToImageFolder.Length + sourceImageName.Length);
            if ((!sourceImageExtension.Equals(".jpg", StringComparison.OrdinalIgnoreCase)
                && !sourceImageExtension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase)
                && !sourceImageExtension.Equals(".png", StringComparison.OrdinalIgnoreCase)))
                return;
            if (!itsFullPath)
            {
                if (pathToImageFolder[0].Equals('/'))
                    pathToImageFolder = pathToImageFolder.Substring(1);
                pathToImageFolder = env.GetStorageFolderFullPath() + pathToImageFolder;
            }
            sourceImageFullName = sourceImageName + sourceImageExtension;
            sourceImageFullPath = pathToImageFolder + sourceImageFullName;
            isExisted = File.Exists(sourceImageFullPath);
            sourceImageShortPath = sourceImageFullPath.Replace(env.GetStorageFolderFullPath(), string.Empty).Insert(0, "/");
            // Если изображения не существует, то удаляем зависимые изображения, если таковые имеются и информацию из БД
            if (!isExisted)
            {
                // Удаляем информацию об изображении из БД
                Image image = db.Images.FirstOrDefault(img => img.ShortPathHash == OtherFunctions.GetHashFromString(sourceImageShortPath) 
                && img.ShortPath.Equals(sourceImageShortPath, StringComparison.Ordinal));
                if (image != null)
                {
                    db.Images.Remove(image);
                    db.SaveChanges();
                }

                ImagesManagementFunctions.DeleteDependentImages(pathToImageFolder, sourceImageFullName);
            }
            else GetSourceImageInfo();
        }
    }
}