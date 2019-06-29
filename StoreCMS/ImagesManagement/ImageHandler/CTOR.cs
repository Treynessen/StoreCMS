using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Treynessen.Extensions;

namespace Treynessen.ImagesManagement
{
    public partial class ImageHandler
    {
        private IHostingEnvironment env;

        private string pathToImageFolder;
        private string sourceImageName;
        private string sourceImageExtension;
        private string sourceImageFullName;
        private string sourceImageFullPath;

        private string pathToImagesInfo;

        private bool isExisted = false;

        public ImageHandler(string pathToImage, bool itsFullPath, IHostingEnvironment env)
        {
            this.env = env;
            pathToImage = pathToImage.Replace('/', '\\');
            if (pathToImage[pathToImage.Length - 1].Equals('\\'))
                return;
            pathToImageFolder = pathToImage.Substring(0, pathToImage.LastIndexOf('\\') + 1);
            sourceImageName = pathToImage.Substring(pathToImageFolder.Length);
            if (!sourceImageName.Contains("."))
                return;
            sourceImageName = sourceImageName.Substring(0, sourceImageName.LastIndexOf('.'));
            sourceImageExtension = pathToImage.Substring(pathToImageFolder.Length + sourceImageName.Length);
            if ((!sourceImageExtension.Equals(".jpg", StringComparison.InvariantCultureIgnoreCase)
                && !sourceImageExtension.Equals(".jpeg", StringComparison.InvariantCultureIgnoreCase)
                && !sourceImageExtension.Equals(".png", StringComparison.InvariantCultureIgnoreCase)))
                return;
            if (!itsFullPath)
            {
                if (pathToImageFolder[0].Equals('\\'))
                    pathToImageFolder = pathToImageFolder.Substring(1);
                pathToImageFolder = env.GetStorageFolderFullPath() + pathToImageFolder;
            }
            sourceImageFullPath = pathToImageFolder + sourceImageName + sourceImageExtension;
            isExisted = File.Exists(sourceImageFullPath);
            pathToImagesInfo = pathToImageFolder + "images.info";
            sourceImageFullName = sourceImageName + sourceImageExtension;
            if (!isExisted && File.Exists(pathToImagesInfo))
            {
                ImagesManagementFunctions.DeleteImageInfoFromInfoFile(pathToImagesInfo, sourceImageFullName);
                ImagesManagementFunctions.DeleteDependentImages(pathToImageFolder, sourceImageFullName);
                return;
            }
            GetSourceImageInfo();
        }
    }
}
