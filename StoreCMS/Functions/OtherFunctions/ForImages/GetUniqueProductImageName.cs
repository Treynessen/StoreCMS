using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Treynessen.Functions
{
    public static partial class OtherFunctions
    {
        public static string GetUniqueProductImageName(string imagesPath, string alias)
        {
            string[] images = null;
            try
            {
                images = Directory.GetFiles(imagesPath, "*.jpg");
            }
            catch (DirectoryNotFoundException) { }
            if (images == null || images.Length == 0)
                return $"{alias}.jpg";
            string fileName = alias;
            bool has = true;
            int index = 0;
            while (has)
            {
                has = false;
                if (index == int.MaxValue)
                {
                    fileName += index.ToString();
                    index = 0;
                }
                string current = $"{imagesPath}{fileName}{(index == 0 ? string.Empty : $"{index.ToString()}")}.jpg";
                foreach (var img in images)
                {
                    if (img.Equals(current))
                    {
                        has = true;
                        break;
                    }
                }
                if (index == 0 && has)
                {
                    fileName += "_";
                }
                if (!has)
                {
                    if (index > 0)
                        fileName += $"{index.ToString()}";
                    Regex regex = new Regex($"{fileName}(_\\d+x\\d+)?(_q\\d{{1,3}})?.jpg$");
                    foreach (var i in images)
                    {
                        if (regex.IsMatch(i))
                            File.Delete(i);
                    }
                    string pathToImagesInfo = $"{imagesPath}images.info";
                    string fileContent = GetFileContent(pathToImagesInfo);
                    if (!string.IsNullOrEmpty(fileContent))
                    {
                        regex = new Regex($"name = {fileName}.jpg; width = \\d+; height = \\d+\n");
                        LinkedList<KeyValuePair<string, string>> listOfChanges =
                            new LinkedList<KeyValuePair<string, string>>(from match in regex.Matches(fileContent)
                                                                         select new KeyValuePair<string, string>(match.Value, string.Empty));
                        if (listOfChanges.Count > 0)
                        {
                            ReplaceContentInFile(pathToImagesInfo, listOfChanges, fileContent);
                        }
                    }
                    fileName += ".jpg";
                }
                ++index;
            }
            return fileName;
        }
    }
}
