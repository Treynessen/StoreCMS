using System.IO;

namespace Treynessen.TemplatesManagement
{
    public static partial class TemplatesManagementFunctions
    {
        public static void WriteCshtmlContentToFile(string pathToTemplatesFolder, string templateName, string chstmlContent)
        {
            Directory.CreateDirectory(pathToTemplatesFolder);
            using (StreamWriter writer = new StreamWriter(pathToTemplatesFolder + templateName + ".cshtml"))
            {
                writer.Write(chstmlContent);
            }
        }
    }
}