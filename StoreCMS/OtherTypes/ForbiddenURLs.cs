using System.Collections.Generic;

namespace Treynessen.OtherTypes
{
    public static class ForbiddenURLs 
    {
        private static List<string> forbiddenURLs;

        public static void SetForbiddenURL(string url)
        {
            if (forbiddenURLs == null)
                forbiddenURLs = new List<string>();

            forbiddenURLs.Add(url);
        }

        public static void SetForbiddenURL(IEnumerable<string> urls)
        {
            if (forbiddenURLs == null)
                forbiddenURLs = new List<string>();

            forbiddenURLs.AddRange(urls);
        }

        public static bool IsCorrectUrl(string url)
        {
            if (forbiddenURLs == null || forbiddenURLs.Count == 0)
                return true;
            return !forbiddenURLs.Contains(url);
        }
    }
}
