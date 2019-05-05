using System.IO;
using System.Linq;
using System.Collections.Generic;
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
                string fileContent = OtherFunctions.GetFileContent(pathToImagesInfo);
                if (string.IsNullOrEmpty(fileContent))
                {
                    File.Delete(pathToImagesInfo);
                }
                else
                {
                    Regex regex = new Regex($"name = {imageFullName}; width = \\d+; height = \\d+\n");
                    MatchCollection matchCollection = regex.Matches(fileContent);
                    LinkedList<KeyValuePair<string, string>> listOfChanges =
                        new LinkedList<KeyValuePair<string, string>>(from match in matchCollection as IEnumerable<Match>
                                                                     select new KeyValuePair<string, string>(match.Value, string.Empty));
                    OtherFunctions.ReplaceContentInFile(pathToImagesInfo, listOfChanges, fileContent);
                }
                OtherFunctions.DeleteCreatedImages(pathToImageFolder, imageFullName);
                return false;
            }
            return true;
        }
    }
}