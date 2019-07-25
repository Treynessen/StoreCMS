using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Treynessen.ImagesManagement
{
    public static partial class ImagesManagementFunctions
    {
        public static void RenameImageAndDependencies(string pathToImages, string oldImageName, string newImageName, string imageExtension)
        {
            if (string.IsNullOrEmpty(pathToImages) || string.IsNullOrEmpty(oldImageName)
                || string.IsNullOrEmpty(newImageName) || !Directory.Exists(pathToImages))
                return;
            if (!pathToImages[pathToImages.Length - 1].Equals('\\'))
                pathToImages = pathToImages.Insert(pathToImages.Length, "\\");
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
        }
    }
}