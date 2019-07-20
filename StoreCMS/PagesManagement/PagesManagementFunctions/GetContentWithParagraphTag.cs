using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Treynessen.PagesManagement
{
    public static partial class PagesManagementFunctions
    {
        private static string GetContentWithParagraphTag(string content)
        {
            if (string.IsNullOrEmpty(content))
                return string.Empty;
            Regex regex = new Regex("((\r)?\n)+");
            MatchCollection forReplacement = regex.Matches(content);
            StringBuilder contentBuilder = new StringBuilder(content);
            if(forReplacement!=null && forReplacement.Count > 0)
            {
                foreach (var match in forReplacement.OrderByDescending(m => m.Value.Length))
                    contentBuilder.Replace(match.Value, "\n");
            }
            if (contentBuilder[contentBuilder.Length - 1].Equals('\n'))
                contentBuilder.Remove(contentBuilder.Length - 1, 1);
            if (contentBuilder[contentBuilder.Length - 1].Equals('\r'))
                contentBuilder.Remove(contentBuilder.Length - 1, 1);
            contentBuilder.Replace("<p>", string.Empty);
            contentBuilder.Replace("</p>", string.Empty);
            contentBuilder.Insert(0, "<p>");
            contentBuilder.Replace("\n", "</p>\n<p>");
            contentBuilder.Insert(contentBuilder.Length, "</p>");
            content = contentBuilder.ToString();
            return content;
        }
    }
}