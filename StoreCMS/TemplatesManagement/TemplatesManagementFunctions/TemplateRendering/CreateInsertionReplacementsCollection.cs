using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Treynessen.Extensions;
using Treynessen.Database.Context;

namespace Treynessen.TemplatesManagement
{
    public static partial class TemplatesManagementFunctions
    {
        private static LinkedList<InsertionReplacement> insertionReplacements = new LinkedList<InsertionReplacement>();

        private static void CreateInsertionReplacementsCollection(CMSDatabase db, string source, string skipChunkName, IHostingEnvironment env)
        {
            if (insertionReplacements.Count > 0)
                insertionReplacements.Clear();
            foreach (var c in GetChunks(db, source, skipChunkName))
            {
                // Вызывать в catch функцию, записывающую информацию об ошибке в лог-файл
                insertionReplacements.AddLast(new InsertionReplacement { Insertion = $"[#{c.Name}]", Replacement = "@{ try { @await Html.PartialAsync(\"" + $"{c.TemplatePath}" + "\", Model) } catch { } }" });
            }

            Regex parser_1 = new Regex(@"\[Product:Images{skip: (?<Type1>\d+)}{left_side: (?<Type2>.*)}{right_side: (?<Type3>.*)}\]");
            foreach (var m in parser_1.Matches(source) as IEnumerable<Match>)
            {
                int skip = Convert.ToInt32(m.Groups[1].Value);
                string leftSubvalue = m.Groups[2].Value.Replace("@", "@@");
                string rightSubvalue = m.Groups[3].Value.Replace("@", "@@");
                string result = "@{ var env = Context.RequestServices.GetService(typeof(Microsoft.AspNetCore.Hosting.IHostingEnvironment)) as Microsoft.AspNetCore.Hosting.IHostingEnvironment; " +
                    "foreach (var imgUrl in ImagesManagementFunctions.GetProductImageUrls(Model as ProductPage, env, " + $"{skip}))" + " { " + $"<text>{leftSubvalue}@(imgUrl){rightSubvalue}</text>" + " } }";
                insertionReplacements.AddLast(new InsertionReplacement { Insertion = m.Value.Replace("@", "@@"), Replacement = result });
            }

            insertionReplacements.AddLast(new InsertionReplacement { Insertion = "[Page:Title]", Replacement = "@(Model != null ? Html.Raw(Model.Title) : Html.Raw(string.Empty))" });
            insertionReplacements.AddLast(new InsertionReplacement { Insertion = "[Page:Name]", Replacement = "@(Model != null ? Html.Raw(Model.PageName) : Html.Raw(string.Empty))" });
            insertionReplacements.AddLast(new InsertionReplacement { Insertion = "[Page:Url]", Replacement = "@(Model != null ? Html.Raw(Model.RequestPath) : Html.Raw(string.Empty))" });
            insertionReplacements.AddLast(new InsertionReplacement { Insertion = "[Page:Breadcrumbs]", Replacement = "@(Model != null ? Html.Raw(Model.BreadcrumbsHtml) : Html.Raw(string.Empty))" });
            insertionReplacements.AddLast(new InsertionReplacement { Insertion = "[Page:Content]", Replacement = "@(Model != null ? Html.Raw(Model.Content) : Html.Raw(string.Empty))" });
            insertionReplacements.AddLast(new InsertionReplacement { Insertion = "[Page:Description]", Replacement = "@(Model != null ? Html.Raw(Model.PageDescription) : Html.Raw(string.Empty))" });
            insertionReplacements.AddLast(new InsertionReplacement { Insertion = "[Page:Keywords]", Replacement = "@(Model != null ? Html.Raw(Model.PageKeywords) : Html.Raw(string.Empty))" });
            insertionReplacements.AddLast(new InsertionReplacement { Insertion = "[Page:RobotsIndex]", Replacement = "@(Model != null ? (Model.IsIndex ? Html.Raw(\"index\") : Html.Raw(\"noindex\")) : Html.Raw(string.Empty))" });
            insertionReplacements.AddLast(new InsertionReplacement { Insertion = "[Page:RobotsFollow]", Replacement = "@(Model != null ? (Model.IsFollow ? Html.Raw(\"follow\") : Html.Raw(\"nofollow\")) : Html.Raw(string.Empty))" });
            // Вызывать в catch функцию, записывающую информацию об ошибке в лог-файл
            insertionReplacements.AddLast(new InsertionReplacement { Insertion = "[ProductList]", Replacement = "@{ if (products != null) { foreach (var p in products) { try { @await Html.PartialAsync(@\"" + $"{env.GetProductBlockCshtmlShortPath()}" + "\", p); } catch { } } } }" });
            insertionReplacements.AddLast(new InsertionReplacement
            {
                Insertion = "[PageButtons]",
                Replacement = "@{ if (products != null) { " +
                "<page-buttons class=\"@Context.Items[\"PaginationStyleName\"]\" " +
                "current-path=\"@Context.Request.Path\" " +
                "current-page=\"@(Context.Items[\"CurrentPage\"] as int?)\" " +
                "pages-count=\"@(Context.Items[\"PagesCount\"] as int?)\"" +
                "></page-buttons>" +
                " } }"
            });
            insertionReplacements.AddLast(new InsertionReplacement { Insertion = "[Product:Name]", Replacement = "@(Model is ProductPage ? Html.Raw((Model as ProductPage).PageName) : Html.Raw(string.Empty))" });
            insertionReplacements.AddLast(new InsertionReplacement { Insertion = "[Product:ShortDescription]", Replacement = "@(Model is ProductPage ? Html.Raw((Model as ProductPage).ShortDescription) : Html.Raw(string.Empty))" });
            insertionReplacements.AddLast(new InsertionReplacement { Insertion = "[Product:Price]", Replacement = "@(Model is ProductPage ? Html.Raw(OtherFunctions.PriceFormatting((Model as ProductPage).Price)) : Html.Raw(string.Empty))" });
            insertionReplacements.AddLast(new InsertionReplacement { Insertion = "[Product:OldPrice]", Replacement = "@(Model is ProductPage ? Html.Raw(OtherFunctions.PriceFormatting((Model as ProductPage).OldPrice)) : Html.Raw(string.Empty))" });
            insertionReplacements.AddLast(new InsertionReplacement { Insertion = "[Product:CurrentPrice]", Replacement = "@(Model is ProductPage && (Model as ProductPage).OldPrice != 0 ? Html.Raw(\"<span>\" + OtherFunctions.PriceFormatting((Model as ProductPage).Price) + \"</span><span>\" + OtherFunctions.PriceFormatting((Model as ProductPage).OldPrice) + \"</span>\") : Html.Raw(\"<span>\" + OtherFunctions.PriceFormatting((Model as ProductPage).Price) + \"</span>\"))" });
            insertionReplacements.AddLast(new InsertionReplacement { Insertion = "[Product:Barcode]", Replacement = "@(Model is ProductPage ? Html.Raw((Model as ProductPage).Barcode) : Html.Raw(string.Empty))" });
            insertionReplacements.AddLast(new InsertionReplacement { Insertion = "[Product:MainImageUrl]", Replacement = $"@(Model is ProductPage ? \"{env.GetProductsImagesFolderSrc()}\" + Model.ID.ToString() + \"/\" + Model.Alias + \".jpg\" : string.Empty)" });
            insertionReplacements.AddLast(new InsertionReplacement { Insertion = "[YEAR]", Replacement = "@(DateTime.Now.Year)" });
        }
    }
}