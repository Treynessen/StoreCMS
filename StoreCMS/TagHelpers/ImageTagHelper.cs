using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Treynessen.ImagesManagement;

namespace Treynessen.TagHelpers
{
    public class ImageTagHelper : TagHelper
    {
        private IHostingEnvironment env;

        public string Src { get; set; }
        public string FullPath { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public int? MaxWidth { get; set; }
        public int? MaxHeight { get; set; }
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
                ImageHandler imageHandler = null;
                if (!string.IsNullOrEmpty(FullPath))
                    imageHandler = new ImageHandler(FullPath, true, env);
                else if (!string.IsNullOrEmpty(Src))
                    imageHandler = new ImageHandler(Src, false, env);
                else return null;

                if (Quality.HasValue)
                    imageHandler.ImageComprassion(Quality.Value);
                imageHandler.ImageResizing(Width, Height, MaxWidth, MaxHeight).ApplySettings();
                return imageHandler.CreatedImageSrc;
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