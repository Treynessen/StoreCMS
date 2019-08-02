using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Treynessen.Database
{
    public static partial class DatabaseInteraction
    {
        private static string GetCorrectUrlPath(string urlPath)
        {
            urlPath = urlPath.ToLower().Replace('\\', '/');
            StringBuilder pathBuilder = new StringBuilder(urlPath);
            if (!pathBuilder[0].Equals('/'))
                pathBuilder.Insert(0, '/');
            Regex regex = new Regex("//+");
            var matches = regex.Matches(urlPath).Select(m => m.Value).OrderByDescending(s => s.Length).ToList();
            foreach (var match in matches)
            {
                pathBuilder.Replace(match, "/");
            }
            if (pathBuilder[pathBuilder.Length - 1].Equals('/'))
                pathBuilder.Remove(pathBuilder.Length - 1, 1);
            return pathBuilder.ToString();
        }
    }
}