using System.IO;
using System.Text;

namespace Treynessen.Functions
{
    public static partial class OtherFunctions
    {
        public static void SourceToCSHTML(string pathToTemplates, string templateName, string source)
        {
            Directory.CreateDirectory(pathToTemplates);
            using (StreamWriter sw = new StreamWriter($"{pathToTemplates}/{templateName}.cshtml"))
            {
                StringBuilder builder = new StringBuilder(source);
                builder.Replace("@", "@@");

                builder.Insert(0, "@model Page\n");
                builder.Replace("[Page:Title]", "@Html.Raw(Model?.Title)");
                builder.Replace("[Page:Breadcrumb]", "@Html.Raw(Model?.BreadcrumbName)");
                builder.Replace("[Page:Content]", "@Html.Raw(Model?.Content)");
                builder.Replace("[Page:PageDescription]", "@Html.Raw(Model?.PageDescription)");
                builder.Replace("[Page:PageKeywords]", "@Html.Raw(Model?.PageKeywords)");
                builder.Replace("[Page:IsRobotIndex]", "@if(Model != null && Model.IsRobotIndex)\n{\n\tindex\n}\nelse\n{\n\tno-index\n}");

                sw.Write(builder.ToString());
            }
        }
    }
}    