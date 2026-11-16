using System;
using System.Buffers.Binary;
using System.IO;

namespace Treynessen.ImagesManagement
{
    internal static class PngExifOrientation
    {
        internal static int Read(Stream input, long start, long length)
        {
            long end = start + length;
            try
            {
                input.Position = start + 8;
                int chunks = 0;
                Span<byte> header = stackalloc byte[8];
                while (input.Position <= end - 12)
                {
                    if (++chunks > 65_536) throw new ImageProcessingException("Превышен лимит 65536 PNG-блоков.");
                    input.ReadExactly(header);
                    uint size = BinaryPrimitives.ReadUInt32BigEndian(header.Slice(0, 4));
                    long chunk = input.Position;
                    if (size > end - chunk - 4) throw Invalid();
                    if (header.Slice(4, 4).SequenceEqual("eXIf"u8)) return ReadTiff(input, chunk, size);
                    if (header.Slice(4, 4).SequenceEqual("IEND"u8)) break;
                    input.Position = chunk + size + 4;
                }
                return 1;
            }
            catch (EndOfStreamException) { throw Invalid(); }
            finally { input.Position = start; }
        }

        private static int ReadTiff(Stream input, long start, uint length)
        {
            // [Комментарий ИИ-агента]: Читаем только короткий тег Orientation первого IFD; смещения и число записей ограничены длиной eXIf, следующие IFD не обходим.
            if (length < 8) throw Invalid();
            Span<byte> header = stackalloc byte[8];
            input.ReadExactly(header);
            bool little = header[0] == (byte)'I' && header[1] == (byte)'I';
            bool big = header[0] == (byte)'M' && header[1] == (byte)'M';
            if ((!little && !big) || U16(header.Slice(2, 2), little) != 42) throw Invalid();
            uint offset = U32(header.Slice(4, 4), little);
            if (offset == 0) return 1;
            if (offset < 8 || offset > length - 2) throw Invalid();
            input.Position = start + offset;
            Span<byte> countBytes = stackalloc byte[2];
            input.ReadExactly(countBytes);
            ushort count = U16(countBytes, little);
            if (count > (start + length - input.Position) / 12) throw Invalid();
            Span<byte> entry = stackalloc byte[12];
            for (int index = 0; index < count; index++)
            {
                input.ReadExactly(entry);
                if (U16(entry.Slice(0, 2), little) != 274) continue;
                if (U16(entry.Slice(2, 2), little) != 3 || U32(entry.Slice(4, 4), little) != 1) throw Invalid();
                int value = U16(entry.Slice(8, 2), little);
                if (value < 1 || value > 8) throw Invalid();
                return value;
            }
            return 1;
        }

        private static ushort U16(ReadOnlySpan<byte> bytes, bool little) => little
            ? BinaryPrimitives.ReadUInt16LittleEndian(bytes) : BinaryPrimitives.ReadUInt16BigEndian(bytes);
        private static uint U32(ReadOnlySpan<byte> bytes, bool little) => little
            ? BinaryPrimitives.ReadUInt32LittleEndian(bytes) : BinaryPrimitives.ReadUInt32BigEndian(bytes);
        private static ImageProcessingException Invalid() => new ImageProcessingException("Некорректная структура EXIF Orientation в PNG.");
    }
}
