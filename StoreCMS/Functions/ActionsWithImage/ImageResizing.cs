using System;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;

namespace Treynessen.Functions
{
    public static partial class ActionsWithImage
    {
        public static string ImageResizing(string path, int? width = null, int? height = null, int? maxWidth = null, int? maxHeight = null)
        {
            if (!width.HasValue && !height.HasValue && !maxWidth.HasValue && !maxHeight.HasValue)
                throw new ArgumentNullException("width or height", "ImageResizing(line 14): width or height doesn't have a value");

            string pathToImageFolder = path.Substring(0, path.LastIndexOf('\\') + 1);
            string sourceFileName = path.Substring(path.LastIndexOf('\\') + 1);
            if (!sourceFileName.Equals(sourceFileName.ToLower(), StringComparison.InvariantCulture))
            {
                sourceFileName = sourceFileName.ToLower();
                File.Move(path, $"{pathToImageFolder}{sourceFileName}");
            }

            string fileNameExtension = path.Substring(path.LastIndexOf('.'));
            if (string.IsNullOrEmpty(fileNameExtension) ||
                (!fileNameExtension.Equals(".jpg", StringComparison.InvariantCultureIgnoreCase)
                && !fileNameExtension.Equals(".jpeg", StringComparison.InvariantCultureIgnoreCase)
                && !fileNameExtension.Equals(".png", StringComparison.InvariantCultureIgnoreCase)))
                throw new NotSupportedException("ImageResizing(line 29): image has an unsupported extension");

            string resultFileName = null;
            if (HasSourceImage(pathToImageFolder, sourceFileName))
            {
                resultFileName = GetImageNameIfHas(pathToImageFolder, path, width: width, height: height, maxWidth: maxWidth, maxHeight: maxHeight);
                if (!string.IsNullOrEmpty(resultFileName))
                    return resultFileName;
            }
            else return null;

            using (Image<Rgba32> source = Image.Load(path))
            {
                int sourceWidth = source.Width;
                int sourceHeight = source.Height;
                if (!width.HasValue && !height.HasValue)
                {
                    if (maxWidth.HasValue || maxHeight.HasValue)
                    {
                        width = sourceWidth;
                        height = sourceHeight;
                    }
                }
                else if (!width.HasValue)
                    width = source.Height > height ? Convert.ToInt32(sourceWidth / ((float)sourceHeight / height)) : Convert.ToInt32(sourceWidth * ((float)height / sourceHeight));
                else if (!height.HasValue)
                    height = source.Width > width ? Convert.ToInt32(sourceHeight / ((float)sourceWidth / width)) : Convert.ToInt32(sourceHeight * ((float)width / sourceWidth));
                ScaleDimension(ref width, ref height, maxWidth, maxHeight);
                if (width.HasValue && height.HasValue)
                {
                    source.Mutate(x => x.Resize(width.Value, height.Value));
                    resultFileName = $"{sourceFileName.Substring(0, sourceFileName.LastIndexOf('.'))}_{width.Value}x{height.Value}{fileNameExtension}";
                    AppendInfoIntoFile(pathToImageFolder, sourceFileName, sourceWidth, sourceHeight);
                    source.Save($"{pathToImageFolder}{resultFileName}");
                }
                return resultFileName;
            }
        }
    }
}