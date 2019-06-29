using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Treynessen.Functions;

namespace Treynessen.ImagesManagement
{
    public static partial class ImagesManagementFunctions
    {
        public static void RenameImageAndDependencies(string pathToImages, string oldImageName,
            string newImageName, bool editImagesInfoFile = true)
        {
            if (string.IsNullOrEmpty(pathToImages) || string.IsNullOrEmpty(oldImageName)
                || string.IsNullOrEmpty(newImageName) || !Directory.Exists(pathToImages))
                return;
            if (!pathToImages[pathToImages.Length - 1].Equals('\\'))
                pathToImages = pathToImages.Insert(pathToImages.Length, "\\");
            string[] images = Directory.GetFiles(pathToImages, $"*{oldImageName}*.jpg");
            Regex regex = new Regex($"{oldImageName}(_\\d+x\\d+)?(_q\\d{{1,3}})?.jpg");
            images = (from img in images
                      where regex.IsMatch(img)
                      select img).ToArray();
            foreach (var img in images)
            {
                string fileEnding = img.Substring(pathToImages.Length + oldImageName.Length);
                File.Move(img, $"{pathToImages}{newImageName}{fileEnding}");
            }
            if (editImagesInfoFile)
            {
                OtherFunctions.ReplaceContentInFile($"{pathToImages}images.info", $"{oldImageName}.jpg", $"{newImageName}.jpg");
            }
        }
    }
}