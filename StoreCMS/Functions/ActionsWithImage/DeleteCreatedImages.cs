using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Treynessen.Functions
{
    public static partial class ActionsWithImage
    {
        private static void DeleteCreatedImages(string pathToImageFolder, string imageFullName)
        {
            var images = Directory.GetFiles(pathToImageFolder);
            string imageName = imageFullName.Substring(0, imageFullName.LastIndexOf('.'));
            string imageExtension = imageFullName.Substring(imageFullName.LastIndexOf('.'));
            Regex regex = new Regex($"{imageName}(_\\d+x\\d+)?(_q\\d{{1,3}})?{imageExtension}$");
            string pathToImage = $"{pathToImageFolder}{imageFullName}";
            var forDelete = images.Where(i => !i.Equals(pathToImage) && regex.IsMatch(i)).ToList();
            foreach (var f in forDelete)
            {
                File.Delete(f);
            }
        }
    }
}