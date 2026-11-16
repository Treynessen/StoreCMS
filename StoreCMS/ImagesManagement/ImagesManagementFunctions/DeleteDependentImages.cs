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
            using (ProductImageOperations.Acquire(pathToImageFolder))
            {
                if (Directory.Exists(pathToImageFolder))
                {
                    string imageName = imageFullName.Substring(0, imageFullName.LastIndexOf('.'));
                    string imageExtension = imageFullName.Substring(imageFullName.LastIndexOf('.'));
                    var images = Directory.GetFiles(pathToImageFolder);
                    Regex regex = new Regex($"\\A{Regex.Escape(imageName)}(_\\d+x\\d+)?(_q\\d{{1,3}})?{Regex.Escape(imageExtension)}\\z");
                    var forDelete = images.Where(i => !Path.GetFileName(i).Equals(imageFullName, StringComparison.Ordinal)
                        && regex.IsMatch(Path.GetFileName(i))).ToList();
                    foreach (var f in forDelete)
                    {
                        File.Delete(f);
                    }
                }
            }
        }
    }
}
