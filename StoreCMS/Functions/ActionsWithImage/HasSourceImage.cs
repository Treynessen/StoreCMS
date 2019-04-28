using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Treynessen.Functions
{
    public static partial class ActionsWithImage
    {
        private static bool HasSourceImage(string pathToImageFolder, string imageFullName)
        {
            if (!File.Exists($"{pathToImageFolder}{imageFullName}"))
            {
                string pathToImagesInfo = $"{pathToImageFolder}images.info";
                LinkedList<Match> forDelete = null;
                string fileContent = null;
                using (StreamReader reader = new StreamReader(pathToImagesInfo))
                {
                    var readToEndTask = reader.ReadToEndAsync();
                    Regex regex = new Regex($"name = {imageFullName}; width = \\d+; height = \\d+\n");
                    fileContent = readToEndTask.Result;
                    MatchCollection matchCollection = regex.Matches(fileContent);
                    forDelete = new LinkedList<Match>(matchCollection);
                }
                if (string.IsNullOrEmpty(fileContent))
                {
                    File.Delete(pathToImagesInfo);
                }
                else
                {
                    using (StreamWriter writer = new StreamWriter(pathToImagesInfo, false))
                    {
                        StringBuilder builder = new StringBuilder(fileContent);
                        foreach (var x in forDelete)
                            builder.Replace(x.Value, string.Empty);
                        writer.Write(builder.ToString());
                    }
                }
                DeleteCreatedImages(pathToImageFolder, imageFullName);
                return false;
            }
            return true;
        }
    }
}