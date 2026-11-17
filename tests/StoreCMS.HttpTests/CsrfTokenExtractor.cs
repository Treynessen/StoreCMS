using System.Net;
using System.Text.RegularExpressions;

namespace StoreCMS.HttpTests;

internal static partial class CsrfTokenExtractor
{
    private const string Failure = "CsrfFormRejected";

    [GeneratedRegex(@"<(?<close>/)?(?<tag>form|input)\b(?<attributes>[^<>]*)>", RegexOptions.IgnoreCase, 100)]
    private static partial Regex Tags();

    [GeneratedRegex("""\G\s*(?<name>[a-zA-Z_:][a-zA-Z0-9_:.-]*)(?:\s*=\s*(?:"(?<value>[^"]*)"|'(?<value>[^']*)'))?""", RegexOptions.None, 100)]
    private static partial Regex Attributes();

    [GeneratedRegex(@"<!--.*?-->|<(script|style)\b[^>]*>.*?</\1\s*>", RegexOptions.IgnoreCase | RegexOptions.Singleline, 100)]
    private static partial Regex InertContent();

    internal static string ExtractLoginToken(string html)
    {
        if (html.Length > 1_048_576) throw new InvalidOperationException(Failure);
        html = InertContent().Replace(html, "");
        bool inForm = false, selected = false;
        int matchingForms = 0, selectors = 0;
        string? token = null;
        foreach (Match tag in Tags().Matches(html))
        {
            string kind = tag.Groups["tag"].Value.ToLowerInvariant();
            bool closing = tag.Groups["close"].Success;
            if (kind == "form" && closing)
            {
                if (!inForm || !string.IsNullOrWhiteSpace(tag.Groups["attributes"].Value))
                    throw new InvalidOperationException(Failure);
                inForm = false;
                selected = false;
                continue;
            }
            if (closing) throw new InvalidOperationException(Failure);
            var attributes = ParseAttributes(tag.Groups["attributes"].Value);
            if (kind == "form")
            {
                if (inForm) throw new InvalidOperationException(Failure);
                inForm = true;
                selected = attributes.GetValueOrDefault("id") == "login-form";
                if (selected)
                {
                    matchingForms++;
                    if (!string.Equals(attributes.GetValueOrDefault("method"), "post", StringComparison.OrdinalIgnoreCase)
                        || attributes.ContainsKey("action"))
                        throw new InvalidOperationException(Failure);
                }
            }
            else if (selected)
            {
                string? name = attributes.GetValueOrDefault("name");
                if (name is "__RequestVerificationToken" or "pageID")
                {
                    if (!string.Equals(attributes.GetValueOrDefault("type"), "hidden", StringComparison.OrdinalIgnoreCase)
                        || attributes.ContainsKey("form"))
                        throw new InvalidOperationException(Failure);
                    string? value = attributes.GetValueOrDefault("value");
                    if (name == "pageID")
                    {
                        if (value != ((int)Treynessen.AdminPanelTypes.AdminPanelPages.LoginForm).ToString(System.Globalization.CultureInfo.InvariantCulture))
                            throw new InvalidOperationException(Failure);
                        selectors++;
                    }
                    else
                    {
                        if (token != null || string.IsNullOrWhiteSpace(value) || value.Any(char.IsControl))
                            throw new InvalidOperationException(Failure);
                        token = value;
                    }
                }
            }
        }
        if (inForm || matchingForms != 1 || selectors != 1 || token == null)
            throw new InvalidOperationException(Failure);
        return token;
    }

    private static Dictionary<string, string> ParseAttributes(string text)
    {
        text = text.TrimEnd();
        if (text.EndsWith('/')) text = text[..^1];
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        int position = 0;
        while (position < text.Length)
        {
            Match match = Attributes().Match(text, position);
            if (!match.Success || match.Index != position || match.Length == 0
                || !result.TryAdd(match.Groups["name"].Value, WebUtility.HtmlDecode(match.Groups["value"].Value)))
                throw new InvalidOperationException(Failure);
            position += match.Length;
        }
        return result;
    }
}