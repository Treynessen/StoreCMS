using System;
using System.Linq;
using System.IO;
using System.Security.Cryptography;
using Treynessen.Functions;

namespace Treynessen.ImagesManagement
{
    public partial class ImageHandler
    {
        private int? sourceImageWidth = null;
        private int? sourceImageHeight = null;
        public string ProcessingError { get; private set; }

        private void GetSourceImageInfo()
        {
            if (!isExisted)
                return;
            Database.Entities.Image image = db.Images.FirstOrDefault(img => img.ShortPathHash == OtherFunctions.GetHashFromString(sourceImageShortPath) 
            && img.ShortPath == sourceImageShortPath);
            using (var input = new FileStream(sourceImageFullPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                sourceContentHash = Convert.ToHexString(SHA256.HashData(input));
                var revision = ReadRevision();
                bool changed = revision == null || revision.Hash != sourceContentHash;
                if (changed)
                {
                    // [Комментарий ИИ-агента]: Timestamp не удостоверяет содержимое; старые варианты удаляем до публикации новой ревизии.
                    ImagesManagementFunctions.DeleteDependentImages(pathToImageFolder, sourceImageFullName);
                    input.Position = 0;
                    try
                    {
                        using (var sourceImage = SkiaImageCodec.Load(input))
                            revision = new SourceRevision { Hash = sourceContentHash, Width = sourceImage.Width, Height = sourceImage.Height };
                    }
                    catch (Exception error) when (error is ImageProcessingException || error is ArgumentException || error is NotSupportedException)
                    {
                        ProcessingError = error is ImageProcessingException ? error.Message : error.GetType().Name;
                        if (image != null) { db.Images.Remove(image); db.SaveChanges(); }
                        if (File.Exists(RevisionPath())) File.Delete(RevisionPath());
                        isExisted = false;
                        return;
                    }
                }
                sourceImageWidth = revision.Width;
                sourceImageHeight = revision.Height;
                if (image == null)
                {
                    image = new Database.Entities.Image { ShortPath = sourceImageShortPath,
                        ShortPathHash = OtherFunctions.GetHashFromString(sourceImageShortPath), FullName = sourceImageFullName };
                    db.Images.Add(image);
                }
                if (db.Entry(image).State == Microsoft.EntityFrameworkCore.EntityState.Added
                    || image.Width != (uint)revision.Width || image.Height != (uint)revision.Height)
                {
                    image.Width = (uint)revision.Width;
                    image.Height = (uint)revision.Height;
                    db.SaveChanges();
                }
                // [Комментарий ИИ-агента]: Ревизию подтверждаем после SQL; сбой допускает адресный повтор, но не выдачу старой производной.
                if (changed) WriteRevision(revision);
            }
        }
    }
}
