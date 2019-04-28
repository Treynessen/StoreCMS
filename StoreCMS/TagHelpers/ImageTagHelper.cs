using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Treynessen.Functions;
using Treynessen.Extensions;

namespace Treynessen.TagHelpers
{
    public class ImageTagHelper : TagHelper
    {
        private IHostingEnvironment env;

        public string Src { get; set; }
        public string FullPath { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public int? Quality { get; set; }
        public string Alt { get; set; }
        public string Style { get; set; }
        public string Class { get; set; }

        public ImageTagHelper(IHostingEnvironment env)
        {
            this.env = env;
        }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            string src = await Task.Run(() =>
            {
                string fullPath = null;
                string resultSrc = null;
                if (!string.IsNullOrEmpty(FullPath))
                {
                    fullPath = FullPath;
                    string storagePath = env.GetStoragePath();
                    if (fullPath.Contains(storagePath))
                    {
                        resultSrc = fullPath.Substring(storagePath.Length - 1).Replace('\\', '/');
                        resultSrc = resultSrc.Substring(0, resultSrc.LastIndexOf('/') + 1);
                    }
                }
                else if (!string.IsNullOrEmpty(Src))
                {
                    fullPath = $"{env.GetStoragePath()}{Src.Substring(1).Replace('/', '\\')}";
                    resultSrc = Src;
                }
                else
                {
                    return Src;
                }
                string imageFullName = null;
                try
                {
                    if ((Width.HasValue || Height.HasValue) && !Quality.HasValue)
                    {
                        imageFullName = ActionsWithImage.ImageResizing(fullPath, Width, Height);
                    }
                    else if ((!Width.HasValue && !Height.HasValue) && Quality.HasValue)
                    {
                        imageFullName = ActionsWithImage.ImageComprassion(fullPath, Quality.Value);
                    }
                    else if ((Width.HasValue || Height.HasValue) && Quality.HasValue)
                    {
                        imageFullName = ActionsWithImage.ImageResizingAndComprassion(fullPath, Quality.Value, Width, Height);
                    }
                    else
                    {
                        imageFullName = FullPath.Substring(FullPath.LastIndexOf('\\') + 1);
                    }
                }
                catch
                {
                    return Src;
                }
                resultSrc += imageFullName;
                return resultSrc;
            });
            output.TagName = "img";
            output.Attributes.SetAttribute("src", src);
            if (!string.IsNullOrEmpty(Alt))
                output.Attributes.SetAttribute("alt", Alt);
            if (!string.IsNullOrEmpty(Style))
                output.Attributes.SetAttribute("style", Style);
            if (!string.IsNullOrEmpty(Class))
                output.Attributes.SetAttribute("class", Class);
        }
    }
}