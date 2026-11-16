namespace Treynessen.ImagesManagement
{
    public partial class ImageHandler
    {
        private int? widthSetting = null;
        private int? heightSetting = null;

        public ImageHandler ImageResizing(int? width = null, int? height = null, int? maxWidth = null, int? maxHeight = null)
        {
            if (isExisted && ((width.HasValue && height.HasValue) || maxWidth.HasValue || maxHeight.HasValue))
            {
                if (width.HasValue && width.Value > 0 && height.HasValue && height.Value > 0)
                {
                    widthSetting = width;
                    heightSetting = height;
                }
                if (maxWidth.HasValue || maxHeight.HasValue)
                {
                    int currentWidth = widthSetting.HasValue ? widthSetting.Value : sourceImageWidth.Value;
                    int currentHeight = heightSetting.HasValue ? heightSetting.Value : sourceImageHeight.Value;
                    ScaleDimension(ref currentWidth, ref currentHeight, maxWidth, maxHeight);
                    if (currentWidth != sourceImageWidth)
                        widthSetting = currentWidth;
                    if (currentHeight != sourceImageHeight)
                        heightSetting = currentHeight;
                    if (!widthSetting.HasValue || !heightSetting.HasValue)
                        widthSetting = heightSetting = null;
                }
            }
            return this;
        }
    }
}