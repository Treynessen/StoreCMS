using System.Collections.Generic;

namespace Treynessen.TemplatesManagement
{
    public static partial class TemplatesManagementFunctions
    {
        private static LinkedList<string> GetValueBetweenSides(string source, string leftSide, string rightSide)
        {
            LinkedList<string> result = new LinkedList<string>();
            if (!string.IsNullOrEmpty(source))
            {
                int beginIndex = 0, endIndex = 0;
                while (beginIndex < source.Length && beginIndex != -1 && endIndex != -1)
                {
                    beginIndex = source.IndexOf(leftSide, endIndex);
                    if (beginIndex != -1)
                    {
                        endIndex = source.IndexOf(rightSide, beginIndex);
                        if (endIndex != -1)
                        {
                            result.AddLast(source.Substring(beginIndex + leftSide.Length, endIndex - beginIndex - leftSide.Length));
                            beginIndex = endIndex + rightSide.Length;
                        }
                    }
                }
            }
            return result;
        }
    }
}