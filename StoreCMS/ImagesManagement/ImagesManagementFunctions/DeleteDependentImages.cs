using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Treynessen.ImagesManagement
{
    public static partial class ImagesManagementFunctions
    {
        public static void DeleteDependentImages(string pathToImageFolder, string imageFullName)
        {
            if (Directory.Exists(pathToImageFolder))
            {
                string imageName = imageFullName.Substring(0, imageFullName.LastIndexOf('.'));
                string imageExtension = imageFullName.Substring(imageFullName.LastIndexOf('.'));
                var images = Directory.GetFiles(pathToImageFolder, $"*{imageName}*{imageExtension}");
                Regex regex = new Regex($"{imageName}(_\\d+x\\d+)?(_q\\d{{1,3}})?{imageExtension}$");
                string pathToImage = $"{pathToImageFolder}{imageFullName}";
                var forDelete = images.Where(i => !i.Equals(pathToImage, StringComparison.Ordinal) && regex.IsMatch(i)).ToList();
                foreach (var f in forDelete)
                {
                    File.Delete(f);
                }
            }
        }
    }
}