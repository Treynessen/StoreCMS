using System.IO;
using System.Text.RegularExpressions;
using Treynessen.Functions;

namespace Treynessen.ImagesManagement
{
    public static partial class ImagesManagementFunctions
    {
        public static void AddImageInfoInInfoFile(string pathToImageFolder, string imageFileFullName, int width, int height)
        {
            if (string.IsNullOrEmpty(pathToImageFolder) 
                || string.IsNullOrEmpty(imageFileFullName) 
                || !Directory.Exists(pathToImageFolder))
                return;
            if (!pathToImageFolder[pathToImageFolder.Length - 1].Equals('\\'))
                pathToImageFolder = pathToImageFolder.Insert(pathToImageFolder.Length, "\\");
            string pathToImagesInfoFile = $"{pathToImageFolder}images.info";
            if (!File.Exists(pathToImagesInfoFile))
                File.Create(pathToImagesInfoFile).Close();
            string fileContent = OtherFunctions.GetFileContent(pathToImagesInfoFile);
            Regex regex = new Regex($"name = {imageFileFullName}; width = \\d+; height = \\d+\n");
            string oldString = regex.Match(fileContent)?.Value;
            string newString = $"name = {imageFileFullName}; width = {width}; height = {height}\n";
            if (!string.IsNullOrEmpty(oldString))
                OtherFunctions.ReplaceContentInFile(pathToImagesInfoFile, oldString, newString, fileContent);
            else
            {
                if (fileContent == null)
                    fileContent = newString;
                else
                    fileContent += newString;
                using (StreamWriter writer = new StreamWriter(pathToImagesInfoFile))
                    writer.Write(fileContent);
            }
        }
    }
}