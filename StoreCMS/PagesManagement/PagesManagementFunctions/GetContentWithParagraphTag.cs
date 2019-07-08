using System.Text;
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
            Regex regex = new Regex("\r\n(\r\n)+");
            MatchCollection forReplacement = regex.Matches(content);
            StringBuilder contentBuilder = new StringBuilder(content);
            if(forReplacement!=null && forReplacement.Count > 0)
            {
                foreach (var match in forReplacement as IEnumerable<Match>)
                    contentBuilder.Replace(match.Value, "\r\n");
            }
            contentBuilder.Replace("<p>", string.Empty);
            contentBuilder.Replace("</p>", string.Empty);
            contentBuilder.Insert(0, "<p>");
            contentBuilder.Replace("\r\n", "</p>\r\n<p>");
            contentBuilder.Insert(contentBuilder.Length, "</p>");
            content = contentBuilder.ToString();
            return content;
        }
    }
}