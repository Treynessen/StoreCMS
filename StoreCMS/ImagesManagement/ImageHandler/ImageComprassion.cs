namespace Treynessen.ImagesManagement
{
    public partial class ImageHandler
    {
        private int? qualitySetting = null;

        public ImageHandler ImageComprassion(int quality)
        {
            if (isExisted && quality >= 0 && quality <= 100)
            {
                qualitySetting = quality;
            }
            return this;
        }
    }
}
