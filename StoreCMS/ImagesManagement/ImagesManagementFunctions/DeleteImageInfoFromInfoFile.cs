using System.IO;
using System.Text.RegularExpressions;
using Treynessen.Functions;

namespace Treynessen.ImagesManagement
{
    public static partial class ImagesManagementFunctions
    {
        public static void DeleteImageInfoFromInfoFile(string pathToImageInfoFile, string imageFileFullName)
        {
            if (string.IsNullOrEmpty(pathToImageInfoFile)
                || string.IsNullOrEmpty(imageFileFullName)
                || !File.Exists(pathToImageInfoFile))
                return;
            string imagesInfoContent = OtherFunctions.GetFileContent(pathToImageInfoFile);
            Regex regex = new Regex($"name = {imageFileFullName}; width = \\d+; height = \\d+\n");
            string stringToDelete = regex.Match(imagesInfoContent)?.Value;
            if (!string.IsNullOrEmpty(stringToDelete))
                imagesInfoContent = imagesInfoContent.Replace(stringToDelete, string.Empty);
            if (string.IsNullOrEmpty(imagesInfoContent) || imagesInfoContent.Equals('\n'))
                File.Delete(pathToImageInfoFile);
            else
            {
                using (StreamWriter writer = new StreamWriter(pathToImageInfoFile))
                    writer.Write(imagesInfoContent);
            }
        }
    }
}