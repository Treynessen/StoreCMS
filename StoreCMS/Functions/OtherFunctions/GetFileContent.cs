using System.IO;

namespace Treynessen.Functions
{
    public static partial class OtherFunctions
    {
        public static string GetFileContent(string pathToFile)
        {
            if (string.IsNullOrEmpty(pathToFile) || !File.Exists(pathToFile))
                return string.Empty;
            string result = null;
            using (StreamReader reader = new StreamReader(pathToFile))
            {
                result = reader.ReadToEnd();
            }
            return result;
        }
    }
}
