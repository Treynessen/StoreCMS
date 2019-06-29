using System;

namespace Treynessen.ImagesManagement
{
    public partial class ImageHandler
    {
        private void ScaleDimension(ref int width, ref int height, int? maxWidth, int? maxHeight)
        {
            double? decreaseScale = null;
            if (maxWidth.HasValue && maxWidth.Value > 0)
            {
                if (width > maxWidth)
                    decreaseScale = width / (double)maxWidth.Value;
            }
            if (maxHeight.HasValue && maxHeight.Value > 0)
            {
                double? tempScale = null;
                if (height > maxHeight)
                    tempScale = height / (double)maxHeight.Value;
                if (decreaseScale.HasValue && tempScale.HasValue && tempScale.Value > decreaseScale.Value)
                    decreaseScale = tempScale;
                else if (!decreaseScale.HasValue && tempScale.HasValue)
                    decreaseScale = tempScale;
            }
            if (decreaseScale.HasValue)
            {
                width = Convert.ToInt32(width / decreaseScale.Value);
                height = Convert.ToInt32(height / decreaseScale.Value);
            }
        }
    }
}
