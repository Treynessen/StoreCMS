using System;
using System.IO;
using System.Text.RegularExpressions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Treynessen.Functions;

namespace Treynessen.ImagesManagement
{
    public partial class ImageHandler
    {
        private int? sourceImageWidth = null;
        private int? sourceImageHeight = null;

        private bool writeSourceImageInfo = false;

        private void GetSourceImageInfo()
        {
            if (!isExisted)
                return;
            if (File.Exists(pathToImagesInfo))
            {
                Regex regex = new Regex($"name = {sourceImageFullName}; width = \\d+; height = \\d+");
                string sourceImageInfo = regex.Match(OtherFunctions.GetFileContent(pathToImagesInfo))?.Value;
                if (!string.IsNullOrEmpty(sourceImageInfo))
                {
                    try
                    {
                        string leftSide = "width = ";
                        string rightSide = ";";
                        int beginIndex = sourceImageInfo.IndexOf(leftSide) + leftSide.Length;
                        int endIndex = sourceImageInfo.LastIndexOf(rightSide, beginIndex + leftSide.Length);
                        sourceImageWidth = Convert.ToInt32(sourceImageInfo.Substring(beginIndex, endIndex - beginIndex));
                        leftSide = "height = ";
                        beginIndex = sourceImageInfo.IndexOf(leftSide, endIndex) + leftSide.Length;
                        sourceImageHeight = Convert.ToInt32(sourceImageInfo.Substring(beginIndex, sourceImageInfo.Length - beginIndex));
                    }
                    catch (FormatException) { }
                }
            }
            if (!sourceImageWidth.HasValue && !sourceImageHeight.HasValue)
            {
                using (Image<Rgba32> sourceImage = Image.Load(sourceImageFullPath))
                {
                    sourceImageWidth = sourceImage.Width;
                    sourceImageHeight = sourceImage.Height;
                    writeSourceImageInfo = true;
                }
            }
        }
    }
}