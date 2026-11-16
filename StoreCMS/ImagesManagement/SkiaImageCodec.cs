using System;
using System.Buffers.Binary;
using System.IO;
using System.Threading;
using SkiaSharp;

namespace Treynessen.ImagesManagement
{
    internal sealed class ImageProcessingException : Exception
    {
        internal ImageProcessingException(string message) : base(message) { }
    }

    internal static class SkiaImageCodec
    {
        internal const long MaxInputBytes = 32L * 1024 * 1024;
        internal const long MaxPixels = 20_000_000;
        internal const int MaxDimension = 16_384;
        private static readonly SemaphoreSlim Slots = new SemaphoreSlim(2, 2);

        internal static DecodedImage Load(Stream input)
        {
            if (input == null || !input.CanRead) throw new ArgumentException("Поток изображения недоступен.", nameof(input));
            if (!Slots.Wait(TimeSpan.FromSeconds(30))) throw new TimeoutException("Превышено время ожидания обработки изображения.");
            bool transferred = false;
            try
            {
                if (!input.CanSeek)
                {
                    using var copy = new MemoryStream();
                    byte[] block = new byte[32768];
                    int count;
                    while ((count = input.Read(block, 0, block.Length)) != 0)
                    {
                        if (copy.Length + count > MaxInputBytes) throw new ImageProcessingException("Изображение превышает 32 МиБ.");
                        copy.Write(block, 0, count);
                    }
                    copy.Position = 0;
                    var decoded = Decode(copy);
                    transferred = true;
                    return decoded;
                }
                var result = Decode(input);
                transferred = true;
                return result;
            }
            finally { if (!transferred) Slots.Release(); }
        }

        private static DecodedImage Decode(Stream input)
        {
            long start = input.Position;
            long length = input.Length - start;
            if (length <= 0 || length > MaxInputBytes) throw new ImageProcessingException("Недопустимый объём изображения: разрешено до 32 МиБ.");
            Span<byte> signature = stackalloc byte[8];
            int read = 0;
            while (read < signature.Length)
            {
                int count = input.Read(signature.Slice(read));
                if (count == 0) break;
                read += count;
            }
            input.Position = start;
            bool png = read >= 8 && signature.SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 });
            bool allowed = read >= 3 && signature[0] == 0xff && signature[1] == 0xd8 && signature[2] == 0xff
                || png
                || read >= 6 && (signature.Slice(0, 6).SequenceEqual("GIF87a"u8) || signature.Slice(0, 6).SequenceEqual("GIF89a"u8))
                || read >= 2 && signature[0] == (byte)'B' && signature[1] == (byte)'M';
            if (!allowed) throw new ImageProcessingException("Разрешены только JPEG, PNG, BMP и GIF.");
            int pngOrientation = png ? PngExifOrientation.Read(input, start, length) : 1;
            using var managed = new SKManagedStream(input, false);
            using var codec = SKCodec.Create(managed);
            if (codec == null || !(codec.EncodedFormat == SKEncodedImageFormat.Jpeg || codec.EncodedFormat == SKEncodedImageFormat.Png
                || codec.EncodedFormat == SKEncodedImageFormat.Bmp || codec.EncodedFormat == SKEncodedImageFormat.Gif))
                throw new ImageProcessingException("Не удалось распознать поддерживаемое изображение.");
            var sourceInfo = codec.Info;
            using var sourceColorSpace = sourceInfo.ColorSpace;
            ValidateDimensions(sourceInfo.Width, sourceInfo.Height);
            using var colorSpace = SKColorSpace.CreateSrgb();
            var info = new SKImageInfo(sourceInfo.Width, sourceInfo.Height, SKColorType.Rgba8888, SKAlphaType.Premul, colorSpace);
            var bitmap = new SKBitmap(info);
            try
            {
                if (bitmap.GetPixels() == IntPtr.Zero) throw new ImageProcessingException("Не удалось выделить память изображения.");
                // [Комментарий ИИ-агента]: Неполный кадр не принимается; GIF преобразуется по первому кадру, как прежний JPEG upload.
                SKCodecResult status = codec.GetPixels(info, bitmap.GetPixels(), new SKCodecOptions(0));
                if (status != SKCodecResult.Success) throw new ImageProcessingException("Декодирование изображения завершилось с ошибкой: " + status + ".");
                bitmap.SetImmutable();
                return new DecodedImage(bitmap, png ? pngOrientation : (int)codec.EncodedOrigin);
            }
            catch { bitmap.Dispose(); throw; }
        }

        private static void ValidateDimensions(int width, int height)
        {
            if (width <= 0 || height <= 0 || width > MaxDimension || height > MaxDimension || checked((long)width * height) > MaxPixels)
                throw new ImageProcessingException("Недопустимые размеры: не более 16384 по стороне и 20 миллионов пикселей.");
        }

        internal sealed class DecodedImage : IDisposable
        {
            private SKBitmap bitmap;
            private readonly int orientation;
            internal int Width => bitmap.Width;
            internal int Height => bitmap.Height;
            internal DecodedImage(SKBitmap bitmap, int orientation) { this.bitmap = bitmap; this.orientation = orientation; }

            internal void Save(Stream output, bool png, int? quality = null, int? width = null, int? height = null)
            {
                if (bitmap == null) throw new ObjectDisposedException(nameof(DecodedImage));
                int w = width ?? Width, h = height ?? Height, q = quality ?? 75;
                // [Комментарий ИИ-агента]: Прежний Resize(0, h) вычислял узкую сторону сам; ScaleDimension может округлить её до нуля.
                if (w == 0 && h > 0) w = Math.Max(1, (int)Math.Round(Width * (h / (double)Height)));
                if (h == 0 && w > 0) h = Math.Max(1, (int)Math.Round(Height * (w / (double)Width)));
                ValidateDimensions(w, h);
                if (q < 0 || q > 100) throw new ArgumentOutOfRangeException(nameof(quality));
                using var colorSpace = SKColorSpace.CreateSrgb();
                using var result = new SKBitmap(new SKImageInfo(w, h, SKColorType.Rgba8888, SKAlphaType.Premul, colorSpace));
                if (result.GetPixels() == IntPtr.Zero) throw new ImageProcessingException("Не удалось выделить память производного изображения.");
                using (var canvas = new SKCanvas(result))
                using (var image = SKImage.FromBitmap(bitmap))
                {
                    // [Комментарий ИИ-агента]: JPEG не хранит alpha; прозрачность накладывается на белый фон, PNG сохраняет alpha.
                    canvas.Clear(png ? SKColors.Transparent : SKColors.White);
                    canvas.DrawImage(image, new SKRect(0, 0, w, h), new SKSamplingOptions(SKCubicResampler.Mitchell));
                    canvas.Flush();
                }
                result.SetImmutable();
                using var encodedImage = SKImage.FromBitmap(result);
                using var encoded = encodedImage.Encode(png ? SKEncodedImageFormat.Png : SKEncodedImageFormat.Jpeg, q);
                if (encoded == null) throw new ImageProcessingException("Не удалось закодировать изображение.");
                if (encoded.Size > MaxInputBytes - 64) throw new ImageProcessingException("Результат обработки превышает 32 МиБ.");
                WriteWithOrientation(output, encoded.AsSpan(), png, orientation);
            }

            public void Dispose()
            {
                var value = Interlocked.Exchange(ref bitmap, null);
                if (value != null) { value.Dispose(); Slots.Release(); }
            }
        }

        private static void WriteWithOrientation(Stream output, ReadOnlySpan<byte> encoded, bool png, int orientation)
        {
            if (orientation <= 1 || orientation > 8) { output.Write(encoded); return; }
            // [Комментарий ИИ-агента]: Сохраняем прежние пиксельные размеры и только доверенный EXIF Orientation, без переноса произвольных метаданных.
            byte[] tiff = { 73, 73, 42, 0, 8, 0, 0, 0, 1, 0, 18, 1, 3, 0, 1, 0, 0, 0, (byte)orientation, 0, 0, 0, 0, 0, 0, 0 };
            if (!png)
            {
                output.Write(encoded.Slice(0, 2));
                output.Write(new byte[] { 255, 225, 0, 34, 69, 120, 105, 102, 0, 0 });
                output.Write(tiff);
                output.Write(encoded.Slice(2));
                return;
            }
            output.Write(encoded.Slice(0, 33));
            Span<byte> length = stackalloc byte[4];
            BinaryPrimitives.WriteUInt32BigEndian(length, (uint)tiff.Length);
            output.Write(length);
            byte[] chunk = new byte[tiff.Length + 4];
            "eXIf"u8.CopyTo(chunk);
            tiff.CopyTo(chunk, 4);
            output.Write(chunk);
            uint crc = uint.MaxValue;
            foreach (byte value in chunk)
            {
                crc ^= value;
                for (int bit = 0; bit < 8; bit++) crc = (crc >> 1) ^ ((crc & 1) != 0 ? 0xedb88320u : 0u);
            }
            BinaryPrimitives.WriteUInt32BigEndian(length, ~crc);
            output.Write(length);
            output.Write(encoded.Slice(33));
        }
    }
}
