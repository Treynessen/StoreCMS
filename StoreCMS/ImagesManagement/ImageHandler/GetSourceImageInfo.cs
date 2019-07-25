using System;
using System.IO;
using System.Linq;
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

        private bool addImageInfoToDB = false;

        private void GetSourceImageInfo()
        {
            if (!isExisted)
                return;
            Database.Entities.Image image = db.Images.FirstOrDefault(img => img.ShortPathHash == OtherFunctions.GetHashFromString(sourceImageShortPath) 
            && img.ShortPath.Equals(sourceImageShortPath, StringComparison.InvariantCulture));
            if (image != null)
            {
                sourceImageWidth = (int)image.Width;
                sourceImageHeight = (int)image.Height;
            }
            if (!sourceImageWidth.HasValue && !sourceImageHeight.HasValue)
            {
                using (Image<Rgba32> sourceImage = Image.Load(sourceImageFullPath))
                {
                    sourceImageWidth = sourceImage.Width;
                    sourceImageHeight = sourceImage.Height;
                    addImageInfoToDB = true;
                }
            }
        }
    }
}