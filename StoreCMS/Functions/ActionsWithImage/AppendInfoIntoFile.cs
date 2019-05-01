using System.IO;
using System.Text.RegularExpressions;

namespace Treynessen.Functions
{
    public static partial class ActionsWithImage
    {
        private static void AppendInfoIntoFile(string pathToImageFolder, string imageFullName, int width, int height)
        {
            string imageInfo = $"name = {imageFullName}; width = {width}; height = {height}\n";
            string pathToImagesInfoFile = $"{pathToImageFolder}images.info";
            string fileContent = OtherFunctions.GetFileContent(pathToImagesInfoFile);
            Regex regex = new Regex($"name = {imageFullName}; width = \\d+; height = \\d+\n");
            Match match = regex.Match(fileContent);
            if (match.Success)
            {
                OtherFunctions.ReplaceContentInFile(pathToImagesInfoFile, match.Value, imageInfo, fileContent);
            }
            else
            {
                using(StreamWriter writer = new StreamWriter(pathToImagesInfoFile, true))
                {
                    writer.Write(imageInfo);
                }
            }
        }
    }
}