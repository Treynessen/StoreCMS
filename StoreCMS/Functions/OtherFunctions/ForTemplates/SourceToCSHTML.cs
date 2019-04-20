using System.IO;
using System.Text;
using Treynessen.Database.Context;

namespace Treynessen.Functions
{
    public static partial class OtherFunctions
    {
        public static void SourceToCSHTML(CMSDatabase db, string path, string name, string source)
        {
            if (path[path.Length - 1] == '/')
                path = path.Substring(0, path.Length - 1);
            bool isChunk = path.EndsWith("/Views/TemplateChunks");

            Directory.CreateDirectory(path);

            using (StreamWriter sw = new StreamWriter($"{path}/{name}.cshtml"))
            {
                if (!string.IsNullOrEmpty(source))
                {
                    StringBuilder builder = new StringBuilder(source);
                    builder.Replace("@", "@@");

                    /* ИНФОРМАЦИЯ О СТРАНИЦЕ */
                    builder.Insert(0, "@model Page\n");
                    builder.Replace("[Page:Title]", "@(Html.Raw(Model?.Title))");
                    builder.Replace("[Page:Breadcrumb]", "@(Html.Raw(Model?.BreadcrumbName))");
                    builder.Replace("[Page:Content]", "@(Html.Raw(Model?.Content))");
                    builder.Replace("[Page:PageDescription]", "@(Html.Raw(Model?.PageDescription))");
                    builder.Replace("[Page:PageKeywords]", "@(Html.Raw(Model?.PageKeywords))");
                    builder.Replace("[Page:IsRobotIndex]", "@(if(Model != null && Model.IsRobotIndex)\n{\n\tindex\n}\nelse\n{\n\tno-index\n})");

                    var chunks = GetChunks(db, source, isChunk ? name : null);
                    foreach (var c in chunks)
                        builder.Replace($"[#{c.Name}]", $"@(await Html.PartialAsync(\"{c.TemplatePath}\", Model))");

                    sw.Write(builder.ToString());
                }
            }
        }
    }
}