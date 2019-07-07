using System.Text;
using Microsoft.AspNetCore.Hosting;
using Treynessen.Extensions;
using Treynessen.Database.Context;

namespace Treynessen.TemplatesManagement
{
    public static partial class TemplatesManagementFunctions
    {
        public static string SourceWithChunksToCSHTML(CMSDatabase db, string source, string modelType,
            IHostingEnvironment env, bool itsChunk = false, string chunkName = null, params string[] additions)
        {
            if (string.IsNullOrEmpty(source))
                return string.Empty;
            StringBuilder cshtmlContentBuilder = new StringBuilder(SourceToCSHTML(source, modelType, env, additions));
            foreach (var c in GetChunks(db, source, itsChunk ? chunkName : null))
                cshtmlContentBuilder.Replace($"[#{c.Name}]", $"@(await Html.PartialAsync(\"{c.TemplatePath}\", Model))");
            return cshtmlContentBuilder.ToString();
        }

        public static string SourceToCSHTML(string source, string modelType, IHostingEnvironment env, params string[] additions)
        {
            if (string.IsNullOrEmpty(source))
                return string.Empty;
            StringBuilder cshtmlContentBuilder = new StringBuilder(source);

            cshtmlContentBuilder.Replace("@", "@@");

            if (source.Contains("[Category:Products]", System.StringComparison.InvariantCulture))
                cshtmlContentBuilder.Insert(0, "@{\n\tList<ProductPage> products = Context.Items[\"products\"] as List<ProductPage>;\n}\n");

            cshtmlContentBuilder.Insert(0, $"@model {modelType}\n");
            if (additions != null && additions.Length > 0)
            {
                for (int i = additions.Length - 1; i >= 0; --i)
                {
                    cshtmlContentBuilder.Insert(0, $"{additions[i]}\n");
                }   
            }

            cshtmlContentBuilder.Replace("[Page:Title]", "@(Model != null ? Html.Raw(Model.Title) : Html.Raw(string.Empty))");
            cshtmlContentBuilder.Replace("[Page:Name]", "@(Model != null ? Html.Raw(Model.PageName) : Html.Raw(string.Empty))");
            cshtmlContentBuilder.Replace("[Page:Url]", "@(Model != null ? Html.Raw(Model.RequestPath) : Html.Raw(string.Empty))");
            cshtmlContentBuilder.Replace("[Page:Breadcrumbs]", "@(Model != null ? Html.Raw(Model.BreadcrumbsHtml) : Html.Raw(string.Empty))");
            cshtmlContentBuilder.Replace("[Page:Content]", "@(Model != null ? Html.Raw(Model.Content) : Html.Raw(string.Empty))");
            cshtmlContentBuilder.Replace("[Page:PageDescription]", "@(Model != null ? Html.Raw(Model.PageDescription) : Html.Raw(string.Empty))");
            cshtmlContentBuilder.Replace("[Page:PageKeywords]", "@(Model != null ? Html.Raw(Model.PageKeywords) : Html.Raw(string.Empty))");
            cshtmlContentBuilder.Replace("[Page:IsIndex]", "@(Model != null ? (Model.IsIndex ? \"index\" : \"noindex\") : Html.Raw(string.Empty))");
            cshtmlContentBuilder.Replace("[Page:IsFollow]", "@(Model != null ? (Model.IsFollow ? \"follow\" : \"nofollow\") : Html.Raw(string.Empty))");

            cshtmlContentBuilder.Replace("[Category:Products]", " @if (products != null) { foreach (var p in products) { @await Html.PartialAsync(@\"" + $"{env.GetConfigsFolderShortPath()}" + "product_block.cshtml\", p); } }");
            cshtmlContentBuilder.Replace("[Category:PageButtons]", " @if (products != null) { <page-buttons class=\"@Context.Items[\"PaginationStyleName\"]\" current-path=\"@Context.Request.Path\" order-by=\"@(Context.Items[\"OrderBy\"] == null ? null : (OrderBy?)Context.Items[\"OrderBy\"])\" current-page=\"@(Context.Items[\"CurrentPage\"] == null ? null : (int?)Context.Items[\"CurrentPage\"])\" pages-count=\"@(Context.Items[\"PagesCount\"] == null ? null : (int?)Context.Items[\"PagesCount\"])\"></page-buttons> }");
            
            cshtmlContentBuilder.Replace("[Product:Name]", "@(Model is ProductPage ? Html.Raw(Model.PageName) : Html.Raw(string.Empty))");
            cshtmlContentBuilder.Replace("[Product:ShortDescription]", "@(Model is ProductPage ? Html.Raw(Model.ShortDescription) : Html.Raw(string.Empty))");
            cshtmlContentBuilder.Replace("[Product:Price]", "@(Model is ProductPage ? Html.Raw(Model.Price) : Html.Raw(string.Empty))");
            cshtmlContentBuilder.Replace("[Product:OldPrice]", "@(Model is ProductPage ? Html.Raw(Model.OldPrice) : Html.Raw(string.Empty))");
            cshtmlContentBuilder.Replace("[Product:MainImage]", $"@(Model is ProductPage ? \"{env.GetProductsImagesFolderSrc()}\" + Model.PreviousPageID.ToString() + Model.ID.ToString() + \"/\" + Model.Alias + \".jpg\" : string.Empty)");

            cshtmlContentBuilder.Replace("[YEAR]", "@(DateTime.Now.Year)");

            return cshtmlContentBuilder.ToString();
        }
    }
}