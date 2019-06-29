namespace Treynessen.TemplatesManagement
{
    public static partial class TemplatesManagementFunctions
    {
        private static int GetNextChunk(int pointer, string source, out string chunkName)
        {
            chunkName = null;
            string opening = "[#";
            string closing = "]";
            int startIndex = source.IndexOf(opening, pointer);
            if (startIndex == -1)
            {
                return startIndex;
            }
            int endIndex = source.IndexOf(closing, startIndex);
            if (endIndex == -1)
                return endIndex;
            int forCheck = source.IndexOf(opening, startIndex + 1);
            if (forCheck != -1 && forCheck > startIndex && forCheck < endIndex)
            {
                while (forCheck < endIndex && forCheck != -1)
                {
                    startIndex = forCheck;
                    forCheck = source.IndexOf(opening, startIndex + 1);
                }
            }
            chunkName = source.Substring(startIndex + 2, endIndex - startIndex - 2);
            return endIndex;
        }
    }
}