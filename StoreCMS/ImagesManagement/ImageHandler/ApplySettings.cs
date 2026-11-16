using System;
using System.IO;
using System.Security.Cryptography;
using Treynessen.Extensions;

namespace Treynessen.ImagesManagement
{
    public partial class ImageHandler
    {
        public string CreatedImageFullPath { get; private set; }
        public string CreatedImageSrc { get; private set; }

        public void ApplySettings()
        {
            if (!isExisted) return;
            using (ProductImageOperations.Acquire(pathToImageFolder))
            {
                string name = sourceImageName;
                if (widthSetting.HasValue && heightSetting.HasValue) name += $"_{widthSetting.Value}x{heightSetting.Value}";
                if (qualitySetting.HasValue) name += $"_q{qualitySetting.Value}";
                name += sourceImageExtension;
                string destination = Path.Combine(pathToImageFolder, name);
                if (!File.Exists(destination))
                {
                    string temporary = Path.Combine(pathToImageFolder, ".derived-" + Guid.NewGuid().ToString("N") + ".tmp");
                    try
                    {
                        using (var input = new FileStream(sourceImageFullPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            if (Convert.ToHexString(SHA256.HashData(input)) != sourceContentHash)
                                throw new IOException("Оригинал изменился во время обработки; требуется новое обращение.");
                            input.Position = 0;
                            using (var source = SkiaImageCodec.Load(input))
                            {
                                using (var output = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                                {
                                    source.Save(output, sourceImageExtension.Equals(".png", StringComparison.OrdinalIgnoreCase),
                                        qualitySetting, widthSetting, heightSetting);
                                    output.Flush(true);
                                }
                            }
                        }
                        // [Комментарий ИИ-агента]: Статический обработчик увидит только законченную производную, без промежуточных JPEG/PNG.
                        File.Move(temporary, destination);
                    }
                    finally { if (File.Exists(temporary)) File.Delete(temporary); }
                }
                CreatedImageFullPath = destination;
                CreatedImageSrc = destination.Replace('\\', '/').Substring(env.GetStorageFolderFullPath().Length - 1)
                    + "?v=" + sourceContentHash;
            }
        }
    }
}
