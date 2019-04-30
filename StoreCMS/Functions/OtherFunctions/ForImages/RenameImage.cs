using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Treynessen.Functions
{
    public static partial class OtherFunctions
    {
        public static void RenameImage(string pathToImages, string oldImageName, string newImageName)
        {
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
            string imagesInfoContent = null;
            string imagesInfoPath = $"{pathToImages}images.info";
            using (StreamReader reader = new StreamReader(imagesInfoPath))
            {
                imagesInfoContent = reader.ReadToEnd();
            }
            Regex imageInfoStringPattern = new Regex($"name = {oldImageName}.jpg; width = \\d+; height = \\d+\n");
            using (StreamWriter writer = new StreamWriter(imagesInfoPath))
            {
                string forReplace = imageInfoStringPattern.Match(imagesInfoContent)?.Value;
                if (!string.IsNullOrEmpty(forReplace))
                {
                    writer.Write(imagesInfoContent.Replace(forReplace, forReplace.Replace(oldImageName, newImageName)));
                }
            }
        }
    }
}