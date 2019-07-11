using System.Text;
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

            if (source.Contains("[Category:Products]", System.StringComparison.InvariantCulture) || source.Contains("[Category:PageButtons]", System.StringComparison.InvariantCulture))
                cshtmlContentBuilder.Insert(0, "@{ List<ProductPage> products = Context.Items[\"products\"] as List<ProductPage>; }\n");

            ReplaceConditionalExpressions(cshtmlContentBuilder);

            cshtmlContentBuilder.Insert(0, $"@model {modelType}\n");
            if (additions != null && additions.Length > 0)
            {
                for (int i = additions.Length - 1; i >= 0; --i)
                {
                    cshtmlContentBuilder.Insert(0, $"{additions[i]}\n");
                }
            }
           
            foreach(var r in insertionReplacements)
            {
                cshtmlContentBuilder.Replace(r.Key, r.Value);
            }

            return cshtmlContentBuilder.ToString();
        }
    }
}