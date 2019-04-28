using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Treynessen.Functions
{
    public static partial class ActionsWithImage
    {
        private static void AppendInfoIntoFile(string pathToImageFolder, string imageFullName, int width, int height)
        {
            string imageInfo = $"name = {imageFullName}; width = {width}; height = {height}\n";
            string pathToImagesInfoFile = $"{pathToImageFolder}images.info";
            LinkedList<string> incorrectInfo = null;
            bool found = false;
            string fileContent = null;
            if (File.Exists(pathToImagesInfoFile))
            {
                using (StreamReader reader = new StreamReader(pathToImagesInfoFile))
                {
                    var readToEndTask = reader.ReadToEndAsync();
                    Regex regex = new Regex($"name = {imageFullName}; width = \\d+; height = \\d+\n");
                    fileContent = readToEndTask.Result;
                    MatchCollection matchCollection = regex.Matches(fileContent);
                    if (matchCollection.Count > 0)
                        incorrectInfo = new LinkedList<string>();
                    foreach (var match in matchCollection as IEnumerable<Match>)
                    {
                        if (match.Value.Equals(imageInfo))
                            found = true;
                        else
                            incorrectInfo.AddLast(match.Value);
                    }
                }
            }
            if (!found || incorrectInfo.Count > 0)
            {
                using (StreamWriter writer = new StreamWriter($"{pathToImageFolder}images.info", false))
                {
                    StringBuilder builder = new StringBuilder(fileContent);
                    if (incorrectInfo != null && incorrectInfo.Count > 0)
                    {
                        foreach (var x in incorrectInfo)
                            builder.Replace(x, string.Empty);
                    }
                    if (!found)
                    {
                        DeleteCreatedImages(pathToImageFolder, imageFullName);
                        builder.Append(imageInfo);
                    }
                    writer.Write(builder.ToString());
                }
            }
        }
    }
}