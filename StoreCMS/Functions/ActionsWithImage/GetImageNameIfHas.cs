using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Treynessen.Functions
{
    public static partial class ActionsWithImage
    {
        private static string GetImageNameIfHas(string pathToImageFolder, string pathToSourceImage,
            int? width = null, int? height = null, int? maxWidth = null, int? maxHeight = null,
            int? quality = null)
        {
            string sourceImageName = pathToSourceImage.Substring(pathToImageFolder.Length);
            string pathToImagesInfo = $"{pathToImageFolder}images.info";
            if (File.Exists(pathToImagesInfo))
            {
                using (StreamReader reader = new StreamReader(pathToImagesInfo))
                {
                    var readToEndTask = reader.ReadToEndAsync();
                    Regex regex = new Regex($"name = {sourceImageName}; width = \\d+; height = \\d+");
                    Match match = regex.Match(readToEndTask.Result);
                    if (match.Success)
                    {
                        if (((width.HasValue || height.HasValue) && (!width.HasValue || !height.HasValue))
                            || (maxWidth.HasValue || maxHeight.HasValue))
                        {
                            string matchValue = match.Value;
                            string leftSide = "width = ";
                            string rightSide = ";";
                            int beginIndex = matchValue.IndexOf(leftSide) + leftSide.Length;
                            int endIndex = matchValue.LastIndexOf(rightSide, beginIndex + leftSide.Length);
                            int sourceImageWidth = Convert.ToInt32(matchValue.Substring(beginIndex, endIndex - beginIndex));
                            leftSide = "height = ";
                            beginIndex = matchValue.IndexOf(leftSide, endIndex) + leftSide.Length;
                            int sourceImageHeight = Convert.ToInt32(matchValue.Substring(beginIndex, matchValue.Length - beginIndex));
                            if (!width.HasValue && !height.HasValue)
                            {
                                if (maxWidth.HasValue || maxHeight.HasValue)
                                {
                                    width = sourceImageWidth;
                                    height = sourceImageHeight;
                                }
                            }
                            else if (!width.HasValue)
                                width = sourceImageHeight > height ? Convert.ToInt32(sourceImageWidth / ((float)sourceImageHeight / height)) : Convert.ToInt32(sourceImageWidth * ((float)height / sourceImageHeight));
                            else if (!height.HasValue)
                                height = sourceImageWidth > width ? Convert.ToInt32(sourceImageHeight / ((float)sourceImageWidth / width)) : Convert.ToInt32(sourceImageHeight * ((float)width / sourceImageWidth));
                            ScaleDimension(ref width, ref height, maxWidth, maxHeight);
                        }
                    }
                    else return null;
                }
            }
            else return null;
            string resultImageFullName = sourceImageName.Substring(0, sourceImageName.LastIndexOf('.'));
            string imageExtension = sourceImageName.Substring(sourceImageName.LastIndexOf('.'));
            if (width.HasValue && height.HasValue)
                resultImageFullName += $"_{width.Value}x{height.Value}";
            if (quality.HasValue)
                resultImageFullName += $"_q{quality.Value}";
            resultImageFullName += imageExtension;
            if (File.Exists($"{pathToImageFolder}{resultImageFullName}"))
                return resultImageFullName;
            else return null;
        }
    }
}