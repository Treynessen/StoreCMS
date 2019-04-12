using Treynessen.Database.Entities;

namespace Treynessen.Functions
{
    public static partial class OtherFunctions
    {
        public static string GetUrl(Page page)
        {
            if (page.RequestPathWithoutAlias.Equals("/") && page.Alias.Equals("index"))
                return "/";
            else if (page.RequestPathWithoutAlias.Equals("/"))
                return $"/{page.Alias}";
            else
                return $"{page.RequestPathWithoutAlias}/{page.Alias}";
        }

        public static string GetUrl(string requestPathWithoutAlias, string alias)
        {
            if (requestPathWithoutAlias.Equals("/") && alias.Equals("index"))
                return "/";
            else if (requestPathWithoutAlias.Equals("/"))
                return $"/{alias}";
            else
                return $"{requestPathWithoutAlias}/{alias}";
        }
    }
}