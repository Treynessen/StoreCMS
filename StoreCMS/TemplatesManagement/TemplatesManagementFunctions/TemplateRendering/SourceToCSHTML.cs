using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Treynessen.Database.Context;

namespace Treynessen.TemplatesManagement
{
    public static partial class TemplatesManagementFunctions
    {
        public static string SourceToCSHTML(CMSDatabase db, string source, string modelType, IHostingEnvironment env, string skipChunkName, params string[] additions)
        {
            if (string.IsNullOrEmpty(source))
                return string.Empty;

            StringBuilder cshtmlContentBuilder = new StringBuilder(source);

            cshtmlContentBuilder.Replace("@", "@@");

            CreateInsertionReplacementsCollection(db, source, skipChunkName, env);

            ReplaceConditionalExpressions(cshtmlContentBuilder);

            foreach (var r in insertionReplacements)
            {
                cshtmlContentBuilder.Replace(r.Insertion, r.Replacement);
            }
            
            Regex parser = new Regex(@"\[Counter id=(?<Type1>\d+)\]");
            if (parser.IsMatch(source))
            {
                cshtmlContentBuilder.Insert(0, "@{ Dictionary<int, int> counters = new Dictionary<int, int>(); }");
                foreach (var m in parser.Matches(source) as IEnumerable<Match>)
                {
                    cshtmlContentBuilder.Replace(m.Value, "@{ if(!counters.ContainsKey(" + $"{m.Groups[1]}" + ")) { counters.Add(" + $"{m.Groups[1]}" + ", 0); } <text>@(++counters[" + $"{m.Groups[1]}" + "])</text> }");
                }
            }

            if (source.Contains("[ProductList]", System.StringComparison.Ordinal) || source.Contains("[Category:PageButtons]", System.StringComparison.Ordinal))
                cshtmlContentBuilder.Insert(0, "@{ List<ProductPage> products = Context.Items[\"products\"] as List<ProductPage>; }\n");

            if (additions != null && additions.Length > 0)
            {
                for (int i = additions.Length - 1; i >= 0; --i)
                {
                    cshtmlContentBuilder.Insert(0, $"{additions[i]}\n");
                }
            }

            cshtmlContentBuilder.Insert(0, $"@model {modelType}\n");

            cshtmlContentBuilder.Append('\n');

            return cshtmlContentBuilder.ToString();
        }
    }
}