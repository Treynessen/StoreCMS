using System.IO;
using Microsoft.AspNetCore.Http;

namespace Treynessen.ImagesManagement
{
    public static partial class ImagesManagementFunctions
    {
        public static void DeleteImage(string pathToImageFolder, string imageFullName, HttpContext context)
        {
            if (string.IsNullOrEmpty(pathToImageFolder) || string.IsNullOrEmpty(imageFullName))
                return;
            if (!pathToImageFolder[pathToImageFolder.Length - 1].Equals('\\'))
                pathToImageFolder = pathToImageFolder.Insert(pathToImageFolder.Length, "\\");
            if (!Directory.Exists(pathToImageFolder))
                return;
            string pathToFile = $"{pathToImageFolder}{imageFullName}";
            if (!File.Exists(pathToFile))
                return;
            File.Delete(pathToFile);
            DeleteImageInfoFromInfoFile($"{pathToImageFolder}images.info", imageFullName);
            // Если у изображения были зависимости (сжатые или ресайзнутые вариации), то удаляем их
            DeleteDependentImages(pathToImageFolder, imageFullName);
        }
    }
}