using Microsoft.AspNetCore.Hosting;
using System.IO;
using Treynessen.Extensions;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Treynessen.ImagesManagement;
using Treynessen.Database.Context;
using System;
using Microsoft.Extensions.Logging;

namespace Treynessen.TagHelpers
{
    public class ImageTagHelper : TagHelper
    {
        private CMSDatabase db;

        private IWebHostEnvironment env;
        private readonly ILogger<ImageTagHelper> logger;

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

        public ImageTagHelper(IWebHostEnvironment env, CMSDatabase db, ILogger<ImageTagHelper> logger = null)
        {
            this.db = db;
            this.env = env;
            this.logger = logger;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (string.IsNullOrEmpty(FullPath) && string.IsNullOrEmpty(Src)) return;
            using (ProductImageOperations.Acquire(Path.GetDirectoryName(!string.IsNullOrEmpty(FullPath) ? FullPath
                : Path.Combine(env.GetStorageFolderFullPath(), (Src ?? "").TrimStart('/')))))
            {
                try
                {
                    ImageHandler imageHandler = null;
                    if (!string.IsNullOrEmpty(FullPath))
                        imageHandler = new ImageHandler(FullPath, true, db, env);
                    else if (!string.IsNullOrEmpty(Src))
                        imageHandler = new ImageHandler(Src, false, db, env);
                    else return;
                    if (Quality.HasValue)
                        imageHandler.ImageComprassion(Quality.Value);
                    imageHandler.ImageResizing(Width, Height, MaxWidth, MaxHeight).ApplySettings();
                    string src = imageHandler.CreatedImageSrc;
                    if (string.IsNullOrEmpty(src))
                    {
                        if (!string.IsNullOrEmpty(imageHandler.ProcessingError))
                            logger?.LogWarning("Изображение отклонено: {Reason}", imageHandler.ProcessingError);
                        output.SuppressOutput();
                        return;
                    }
                    output.TagName = "img";
                    output.Attributes.SetAttribute("src", src);
                    if (!string.IsNullOrEmpty(Alt))
                        output.Attributes.SetAttribute("alt", Alt);
                    if (!string.IsNullOrEmpty(Style))
                        output.Attributes.SetAttribute("style", Style);
                    if (!string.IsNullOrEmpty(Class))
                        output.Attributes.SetAttribute("class", Class);
                }
                catch (Exception error)
                {
                    // [Комментарий ИИ-агента]: Повреждённый файл или неподтверждённая метадата не должны возвращать старую картинку либо обрушать страницу.
                    logger?.LogWarning("Не удалось обработать изображение: {ErrorType}", error.GetType().Name);
                    output.SuppressOutput();
                }
            }
        }
    }
}
