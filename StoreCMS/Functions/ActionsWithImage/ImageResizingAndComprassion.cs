using System;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;

namespace Treynessen.Functions
{
    public static partial class ActionsWithImage
    {
        public static string ImageResizingAndComprassion(string path, int quality,
            int? width = null, int? height = null, int? maxWidth = null, int? maxHeight = null)
        {
            if (quality < 0 || quality > 100)
                throw new ArgumentException("quality", "ImageResizingAndComprassion(line 17): quality has an incorrect value");

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
                throw new ArgumentException("ImageResizingAndComprassion(line 33): image has an unsupported extension");

            string resultFileName = null;
            if (HasSourceImage(pathToImageFolder, sourceFileName))
            {
                resultFileName = GetImageNameIfHas(pathToImageFolder, path, width: width, height: height, maxWidth: maxWidth, maxHeight: maxHeight, quality: quality);
                if (!string.IsNullOrEmpty(resultFileName))
                    return resultFileName;
            }
            else return null;

            IImageEncoder imageEncoder = null;
            switch (fileNameExtension.ToLower())
            {
                case ".jpg":
                case ".jpeg":
                    var jpegEncoder = new JpegEncoder();
                    jpegEncoder.Quality = quality;
                    imageEncoder = jpegEncoder;
                    break;
                case ".png":
                    var pngEncoder = new PngEncoder();
                    pngEncoder.CompressionLevel = quality / 11 + 1;
                    imageEncoder = pngEncoder;
                    break;
            }
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
                resultFileName = $"{sourceFileName.Substring(0, sourceFileName.LastIndexOf('.'))}";
                if (width.HasValue && height.HasValue)
                {
                    source.Mutate(x => x.Resize(width.Value, height.Value));
                    resultFileName += $"_{width.Value}x{height.Value}";
                }
                resultFileName += $"_q{quality}{fileNameExtension}";
                AppendInfoIntoFile(pathToImageFolder, sourceFileName, sourceWidth, sourceHeight);
                source.Save($"{pathToImageFolder}{resultFileName}", imageEncoder);
                return resultFileName;
            }
        }
    }
}