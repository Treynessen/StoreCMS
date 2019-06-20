using System.IO;

namespace Treynessen.Functions
{
    public static partial class OtherFunctions
    {
        public static string GetProductTemplateSource(string pathToOriginFile)
        {
            string result = null;
            if (File.Exists(pathToOriginFile))
            {
                using (StreamReader reader = new StreamReader(pathToOriginFile))
                {
                    result = reader.ReadToEnd();
                }
            }
            return result;
        }
    }
}