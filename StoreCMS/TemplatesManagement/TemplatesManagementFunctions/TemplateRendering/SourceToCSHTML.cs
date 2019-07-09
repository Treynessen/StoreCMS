using System.Text;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Treynessen.Functions;
using Treynessen.Extensions;
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

            if (source.Contains("[Category:Products]", System.StringComparison.InvariantCulture))
                cshtmlContentBuilder.Insert(0, "@{ List<ProductPage> products = Context.Items[\"products\"] as List<ProductPage>; }\n");

            cshtmlContentBuilder.Insert(0, $"@model {modelType}\n");
            if (additions != null && additions.Length > 0)
            {
                for (int i = additions.Length - 1; i >= 0; --i)
                {
                    cshtmlContentBuilder.Insert(0, $"{additions[i]}\n");
                }
            }

            foreach (var c in GetChunks(db, source, skipChunkName))
            {
                // Вызывать в catch функцию, записывающую информацию об ошибке в лог-файл
                cshtmlContentBuilder.Replace($"[#{c.Name}]", "@{ try { @await Html.PartialAsync(\"" + $"{c.TemplatePath}" + "\", Model) } catch { } }");
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

            // Вызывать в catch функцию, записывающую информацию об ошибке в лог-файл
            cshtmlContentBuilder.Replace("[Category:Products]", "@{ if (products != null) { foreach (var p in products) { try { @await Html.PartialAsync(@\"" + $"{env.GetConfigsFolderShortPath()}" + "product_block.cshtml\", p); } catch { } } } }");
            cshtmlContentBuilder.Replace(
                "[Category:PageButtons]",
                "@{ if (products != null) { " +
                "<page-buttons class=\"@Context.Items[\"PaginationStyleName\"]\" " +
                "current-path=\"@Context.Request.Path\" " +
                "order-by=\"@(Context.Items[\"OrderBy\"] as OrderBy?)\" " +
                "current-page=\"@(Context.Items[\"CurrentPage\"] as int?)\" " +
                "pages-count=\"@(Context.Items[\"PagesCount\"] as int?)\"" +
                "></page-buttons>" +
                " } }"
            );

            cshtmlContentBuilder.Replace("[Product:Name]", "@(Model is ProductPage ? Html.Raw((Model as ProductPage).PageName) : Html.Raw(string.Empty))");
            cshtmlContentBuilder.Replace("[Product:ShortDescription]", "@(Model is ProductPage ? Html.Raw((Model as ProductPage).ShortDescription) : Html.Raw(string.Empty))");
            cshtmlContentBuilder.Replace("[Product:Price]", "@(Model is ProductPage ? Html.Raw(OtherFunctions.FormatPrice((Model as ProductPage).Price)) : Html.Raw(string.Empty))");
            cshtmlContentBuilder.Replace("[Product:OldPrice]", "@(Model is ProductPage ? Html.Raw(OtherFunctions.FormatPrice((Model as ProductPage).OldPrice)) : Html.Raw(string.Empty))");
            cshtmlContentBuilder.Replace("[Product:CurrentPrice]", "@(Model is ProductPage && (Model as ProductPage).OldPrice != 0 ? Html.Raw(\"<span>\" + OtherFunctions.FormatPrice((Model as ProductPage).Price) + \"</span><span>\" + OtherFunctions.FormatPrice((Model as ProductPage).OldPrice) + \"</span>\") : Html.Raw(\"<span>\" + OtherFunctions.FormatPrice((Model as ProductPage).Price) + \"</span>\"))");
            cshtmlContentBuilder.Replace("[Product:MainImage]", $"@(Model is ProductPage ? \"{env.GetProductsImagesFolderSrc()}\" + Model.PreviousPageID.ToString() + Model.ID.ToString() + \"/\" + Model.Alias + \".jpg\" : string.Empty)");

            var specialProductValues = OtherFunctions.GetValuesBetweenSides(cshtmlContentBuilder.ToString(), "[Product:IfSpecial(", ")]");
            if (specialProductValues.Count > 0)
            {
                foreach (var v in specialProductValues)
                {
                    cshtmlContentBuilder.Replace($"[Product:IfSpecial({v})]", $"@(Model is ProductPage && (Model as ProductPage).SpecialProduct ? Html.Raw(\"{v.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("@@", "@")}\") : Html.Raw(string.Empty))");
                }
            }

            var stockProductValues = OtherFunctions.GetValuesBetweenSides(cshtmlContentBuilder.ToString(), "[Product:IfStock(", ")]");
            if (stockProductValues.Count > 0)
            {
                foreach (var v in stockProductValues)
                {
                    cshtmlContentBuilder.Replace($"[Product:IfStock({v})]", $"@(Model is ProductPage && (Model as ProductPage).Price > 0 && (Model as ProductPage).OldPrice > 0 ? Html.Raw(\"{v.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("@@", "@")}\") : Html.Raw(string.Empty))");
                }
            }

            cshtmlContentBuilder.Replace("[YEAR]", "@(DateTime.Now.Year)");

            return cshtmlContentBuilder.ToString();
        }
    }
}