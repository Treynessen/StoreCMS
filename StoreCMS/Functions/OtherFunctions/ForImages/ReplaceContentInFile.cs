using System.IO;
using System.Text;
using System.Collections.Generic;

namespace Treynessen.Functions
{
    public static partial class OtherFunctions
    {
        public static void ReplaceContentInFile(string path, LinkedList<KeyValuePair<string, string>> listOfChanges, string fileContent = null)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
                return;
            if (string.IsNullOrEmpty(fileContent))
                fileContent = GetFileContent(path);
            StringBuilder builder = new StringBuilder(fileContent);
            using (StreamWriter writer = new StreamWriter(path))
            {
                foreach (var c in listOfChanges)
                {
                    builder.Replace(c.Key, c.Value);
                }
                writer.Write(builder.ToString());
            }
        }

        public static void ReplaceContentInFile(string path, string oldString, string newString, string fileContent = null)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
                return;
            if (string.IsNullOrEmpty(fileContent))
                fileContent = GetFileContent(path);
            StringBuilder builder = new StringBuilder(fileContent);
            using (StreamWriter writer = new StreamWriter(path))
            {
                builder.Replace(oldString, newString);
                writer.Write(builder.ToString());
            }
        }
    }
}
