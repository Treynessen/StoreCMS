using System;

namespace Treynessen.Functions
{
    public static partial class ActionsWithImage
    {
        private static void ScaleDimension(ref int? width, ref int? height, int? maxWidth, int? maxHeight)
        {
            float? decreaseScale = null;
            if (maxWidth.HasValue)
            {
                if (width > maxWidth)
                    decreaseScale = width.Value / (float)maxWidth.Value;
            }
            if (maxHeight.HasValue)
            {
                float? tempScale = null;
                if (height > maxHeight)
                    tempScale = height.Value / (float)maxHeight.Value;
                if (decreaseScale.HasValue && tempScale.HasValue && tempScale.Value > decreaseScale.Value)
                    decreaseScale = tempScale;
                else if (!decreaseScale.HasValue && tempScale.HasValue)
                    decreaseScale = tempScale;
            }
            if (decreaseScale.HasValue)
            {
                width = Convert.ToInt32(width.Value / decreaseScale.Value);
                height = Convert.ToInt32(height.Value / decreaseScale.Value);
            }
        }
    }
}
