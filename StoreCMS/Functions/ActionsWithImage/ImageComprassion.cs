using System;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;

namespace Treynessen.Functions
{
    public static partial class ActionsWithImage
    {
        public static string ImageComprassion(string path, int quality)
        {
            if (quality < 0 || quality > 100)
                throw new ArgumentException("quality", "ImageComprassion(line 16): quality has an incorrect value");

            string pathToImageFolder = path.Substring(0, path.LastIndexOf('\\') + 1);
            string sourceFileName = path.Substring(path.LastIndexOf('\\') + 1);
            if (!sourceFileName.Equals(sourceFileName.ToLower()))
            {
                sourceFileName = sourceFileName.ToLower();
                File.Move(path, $"{pathToImageFolder}{sourceFileName}");
            }

            string fileNameExtension = path.Substring(path.LastIndexOf('.'));
            if (string.IsNullOrEmpty(fileNameExtension) ||
                (!fileNameExtension.Equals(".jpg", StringComparison.CurrentCultureIgnoreCase)
                && !fileNameExtension.Equals(".jpeg", StringComparison.CurrentCultureIgnoreCase)
                && !fileNameExtension.Equals(".png", StringComparison.CurrentCultureIgnoreCase)))
                throw new ArgumentException("ImageComprassion(line 31): image has an unsupported extension");

            string resultFileName = null;
            if (HasSourceImage(pathToImageFolder, sourceFileName))
            {
                resultFileName = GetImageNameIfHas(pathToImageFolder, path, quality: quality);
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
                resultFileName = $"{sourceFileName.Substring(0, sourceFileName.LastIndexOf('.'))}_q{quality}{fileNameExtension}";
                AppendInfoIntoFile(pathToImageFolder, sourceFileName, source.Width, source.Height);
                source.Save($"{pathToImageFolder}{resultFileName}", imageEncoder);
                return resultFileName;
            }
        }
    }
}