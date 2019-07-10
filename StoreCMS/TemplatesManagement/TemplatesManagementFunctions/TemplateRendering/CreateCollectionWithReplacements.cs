using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Treynessen.Extensions;
using Treynessen.Database.Context;

namespace Treynessen.TemplatesManagement
{
    public static partial class TemplatesManagementFunctions
    {
        private static LinkedList<KeyValuePair<string, string>> replacements = new LinkedList<KeyValuePair<string, string>>();

        private static void CreateCollectionWithReplacements(CMSDatabase db, string source, string skipChunkName, IHostingEnvironment env)
        {
            if (replacements.Count > 0)
                replacements.Clear();
            foreach (var c in GetChunks(db, source, skipChunkName))
            {
                // Вызывать в catch функцию, записывающую информацию об ошибке в лог-файл
                replacements.AddLast(new KeyValuePair<string, string>($"[#{c.Name}]", "@{ try { @await Html.PartialAsync(\"" + $"{c.TemplatePath}" + "\", Model) } catch { } }"));
            }

            replacements.AddLast(new KeyValuePair<string, string>("[Page:Title]", "@(Model != null ? Html.Raw(Model.Title) : Html.Raw(string.Empty))"));
            replacements.AddLast(new KeyValuePair<string, string>("[Page:Name]", "@(Model != null ? Html.Raw(Model.PageName) : Html.Raw(string.Empty))"));
            replacements.AddLast(new KeyValuePair<string, string>("[Page:Url]", "@(Model != null ? Html.Raw(Model.RequestPath) : Html.Raw(string.Empty))"));
            replacements.AddLast(new KeyValuePair<string, string>("[Page:Breadcrumbs]", "@(Model != null ? Html.Raw(Model.BreadcrumbsHtml) : Html.Raw(string.Empty))"));
            replacements.AddLast(new KeyValuePair<string, string>("[Page:Content]", "@(Model != null ? Html.Raw(Model.Content) : Html.Raw(string.Empty))"));
            replacements.AddLast(new KeyValuePair<string, string>("[Page:PageDescription]", "@(Model != null ? Html.Raw(Model.PageDescription) : Html.Raw(string.Empty))"));
            replacements.AddLast(new KeyValuePair<string, string>("[Page:PageKeywords]", "@(Model != null ? Html.Raw(Model.PageKeywords) : Html.Raw(string.Empty))"));
            replacements.AddLast(new KeyValuePair<string, string>("[Page:IsIndex]", "@(Model != null ? (Model.IsIndex ? \"index\" : \"noindex\") : Html.Raw(string.Empty))"));
            replacements.AddLast(new KeyValuePair<string, string>("[Page:IsFollow]", "@(Model != null ? (Model.IsFollow ? \"follow\" : \"nofollow\") : Html.Raw(string.Empty))"));
            // Вызывать в catch функцию, записывающую информацию об ошибке в лог-файл
            replacements.AddLast(new KeyValuePair<string, string>("[Category:Products]", "@{ if (products != null) { foreach (var p in products) { try { @await Html.PartialAsync(@\"" + $"{env.GetProductBlockCshtmlFullPath()}" + "\", p); } catch { } } } }"));
            replacements.AddLast(new KeyValuePair<string, string>(
                "[Category:PageButtons]",
                "@{ if (products != null) { " +
                "<page-buttons class=\"@Context.Items[\"PaginationStyleName\"]\" " +
                "current-path=\"@Context.Request.Path\" " +
                "order-by=\"@(Context.Items[\"OrderBy\"] as OrderBy?)\" " +
                "current-page=\"@(Context.Items[\"CurrentPage\"] as int?)\" " +
                "pages-count=\"@(Context.Items[\"PagesCount\"] as int?)\"" +
                "></page-buttons>" +
                " } }"
            ));
            replacements.AddLast(new KeyValuePair<string, string>("[Product:Name]", "@(Model is ProductPage ? Html.Raw((Model as ProductPage).PageName) : Html.Raw(string.Empty))"));
            replacements.AddLast(new KeyValuePair<string, string>("[Product:ShortDescription]", "@(Model is ProductPage ? Html.Raw((Model as ProductPage).ShortDescription) : Html.Raw(string.Empty))"));
            replacements.AddLast(new KeyValuePair<string, string>("[Product:Price]", "@(Model is ProductPage ? Html.Raw(OtherFunctions.FormatPrice((Model as ProductPage).Price)) : Html.Raw(string.Empty))"));
            replacements.AddLast(new KeyValuePair<string, string>("[Product:OldPrice]", "@(Model is ProductPage ? Html.Raw(OtherFunctions.FormatPrice((Model as ProductPage).OldPrice)) : Html.Raw(string.Empty))"));
            replacements.AddLast(new KeyValuePair<string, string>("[Product:CurrentPrice]", "@(Model is ProductPage && (Model as ProductPage).OldPrice != 0 ? Html.Raw(\"<span>\" + OtherFunctions.FormatPrice((Model as ProductPage).Price) + \"</span><span>\" + OtherFunctions.FormatPrice((Model as ProductPage).OldPrice) + \"</span>\") : Html.Raw(\"<span>\" + OtherFunctions.FormatPrice((Model as ProductPage).Price) + \"</span>\"))"));
            replacements.AddLast(new KeyValuePair<string, string>("[Product:MainImage]", $"@(Model is ProductPage ? \"{env.GetProductsImagesFolderSrc()}\" + Model.PreviousPageID.ToString() + Model.ID.ToString() + \"/\" + Model.Alias + \".jpg\" : string.Empty)"));
            replacements.AddLast(new KeyValuePair<string, string>("[YEAR]", "@(DateTime.Now.Year)"));
        }
    }
}