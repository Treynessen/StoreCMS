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

        private IWebHostEnvironment env;

        private string pathToImageFolder;
        private string sourceImageName;
        private string sourceImageExtension;
        private string sourceImageFullName;
        private string sourceImageFullPath;
        private string sourceImageShortPath;

        private bool isExisted = false;

        public ImageHandler(string pathToImage, bool itsFullPath, CMSDatabase db,  IWebHostEnvironment env)
        {
            this.db = db;
            this.env = env;
            if (string.IsNullOrEmpty(pathToImage)) return;
            string storageRoot = Path.GetFullPath(env.GetStorageFolderFullPath()).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                + Path.DirectorySeparatorChar;
            string canonical;
            try
            {
                canonical = Path.GetFullPath(itsFullPath ? pathToImage
                    : Path.Combine(storageRoot, pathToImage.TrimStart('/', '\\')));
            }
            catch (ArgumentException) { return; }
            catch (NotSupportedException) { return; }
            catch (PathTooLongException) { return; }
            // [Комментарий ИИ-агента]: Проверяем канонический путь до чтения файла, метаданных и создания производных.
            if (canonical.Length <= storageRoot.Length
                || !canonical.StartsWith(storageRoot, OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal)) return;
            pathToImage = (itsFullPath ? canonical : canonical.Substring(storageRoot.Length)).Replace('\\', '/');
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
                if (pathToImageFolder.Length > 0 && pathToImageFolder[0].Equals('/'))
                    pathToImageFolder = pathToImageFolder.Substring(1);
                pathToImageFolder = env.GetStorageFolderFullPath() + pathToImageFolder;
            }
            sourceImageFullName = sourceImageName + sourceImageExtension;
            sourceImageFullPath = pathToImageFolder + sourceImageFullName;
            using (ProductImageOperations.Acquire(pathToImageFolder))
            {
                isExisted = File.Exists(sourceImageFullPath);
                sourceImageShortPath = sourceImageFullPath.Substring(env.GetStorageFolderFullPath().Length).Insert(0, "/");
                // Если изображения не существует, то удаляем зависимые изображения, если таковые имеются и информацию из БД
                if (!isExisted)
                {
                    // Удаляем информацию об изображении из БД
                    Image image = db.Images.FirstOrDefault(img => img.ShortPathHash == OtherFunctions.GetHashFromString(sourceImageShortPath)
                    && img.ShortPath == sourceImageShortPath);
                    if (image != null)
                    {
                        db.Images.Remove(image);
                        db.SaveChanges();
                    }

                    ImagesManagementFunctions.DeleteDependentImages(pathToImageFolder, sourceImageFullName);
                    if (File.Exists(RevisionPath())) File.Delete(RevisionPath());
                }
                else GetSourceImageInfo();
            }
        }
    }
}
