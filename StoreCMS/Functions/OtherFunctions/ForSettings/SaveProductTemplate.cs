using System;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Treynessen.Extensions;

namespace Treynessen.Functions
{
    public static partial class OtherFunctions
    {
        public static void SaveProductTemplate(string productTemplate, IHostingEnvironment env)
        {
            string pathToConfigurations = env.GetConfigurationsPath();
            bool hasChanges = true;
            string oldTemplate = GetFileContent($"{pathToConfigurations}\\product_template.source");
            if (oldTemplate.Equals(productTemplate, StringComparison.InvariantCulture))
                hasChanges = false;
            if (hasChanges)
            {
                using (StreamWriter writer = new StreamWriter($"{pathToConfigurations}\\product_template.source", false))
                {
                    writer.Write(productTemplate);
                }
                StringBuilder templateBuilder = new StringBuilder(productTemplate);
                templateBuilder.Insert(0, "@model ProductPage\n");
                templateBuilder.Insert(0, "@using Treynessen.Database.Entities;\n");
                templateBuilder.Insert(0, "@using Treynessen.Functions;\n");
                templateBuilder.Insert(0, "@addTagHelper Treynessen.TagHelpers.ImageTagHelper, StoreCMS\n");
                templateBuilder.Replace("[Product:Name]", "@(Html.Raw(Model?.PageName))");
                templateBuilder.Replace("[Product:ShortDescription]", "@(Html.Raw(Model?.ShortDescription))");
                templateBuilder.Replace("[Product:Price]", "@(Html.Raw(Model?.Price))");
                templateBuilder.Replace("[Product:OldPrice]", "@(Html.Raw(Model?.OldPrice))");
                templateBuilder.Replace("[Product:Url]", "@(Html.Raw(OtherFunctions.GetUrl(Model)))");
                templateBuilder.Replace("[Product:MainImage]", $"{env.GetProductsImagesPath(true)}@(Model?.PreviousPageID)@(Model?.ID)/@(Model?.Alias).jpg");
                using (StreamWriter writer = new StreamWriter($"{pathToConfigurations}\\product_template.cshtml", false))
                {
                    writer.Write(templateBuilder.ToString());
                }
            }
        }
    }
}