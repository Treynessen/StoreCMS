using SkiaSharp;
using Treynessen.ImagesManagement;
using Xunit;

namespace StoreCMS.ComponentTests;

[Trait("Category", "Native")]
[Trait("Area", "SkiaSharp")]
public sealed class ImageCodecTests : FileTest
{
    private string Jpeg()
    {
        string path = Path.Combine(Root, "source.jpg");
        using var bitmap = new SKBitmap(24, 12);
        bitmap.Erase(SKColors.CornflowerBlue);
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, 90);
        using var output = File.Create(path);
        data.SaveTo(output);
        return path;
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void NativeDecodeResizeSaveChecksDimensionsFormatAndReleasesFiles(bool png)
    {
        string source = Jpeg();
        string target = Path.Combine(Root, png ? "result.png" : "result.jpg");
        using (var input = File.OpenRead(source))
        using (var decoded = SkiaImageCodec.Load(input))
        {
            Assert.Equal(24, decoded.Width);
            Assert.Equal(12, decoded.Height);
            using var output = File.Create(target);
            decoded.Save(output, png, 80, 8, 4);
        }
        using (var stream = File.OpenRead(target))
        using (var codec = SKCodec.Create(stream))
        {
            Assert.Equal(png ? SKEncodedImageFormat.Png : SKEncodedImageFormat.Jpeg, codec.EncodedFormat);
            Assert.Equal(8, codec.Info.Width);
            Assert.Equal(4, codec.Info.Height);
        }
        using (new FileStream(source, FileMode.Open, FileAccess.ReadWrite, FileShare.None)) { }
        using (new FileStream(target, FileMode.Open, FileAccess.ReadWrite, FileShare.None)) { }
        File.Delete(source);
        File.Delete(target);
        Assert.False(File.Exists(source));
        Assert.False(File.Exists(target));
    }

    [Theory]
    [InlineData("empty")]
    [InlineData("jpeg")]
    [InlineData("svg")]
    public void InvalidInputRejectedAndDecodeSlotReleased(string kind)
    {
        byte[] bytes = kind switch { "jpeg" => [255, 216, 255, 0, 0, 0, 0, 0], "svg" => "<svg/>"u8.ToArray(), _ => [] };
        string path = Write("invalid.jpg");
        File.WriteAllBytes(path, bytes);
        for (int i = 0; i < 3; i++)
        {
            using var input = File.OpenRead(path);
            Assert.Throws<ImageProcessingException>(() => SkiaImageCodec.Load(input));
        }
        using (new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None)) { }
        string valid = Jpeg();
        using var good = File.OpenRead(valid);
        using var decoded = SkiaImageCodec.Load(good);
        Assert.Equal(24, decoded.Width);
    }

    [Fact]
    public void OutputFailureReleasesResourcesAndAllowsExplicitRetry()
    {
        string source = Jpeg();
        string target = Write("result.jpg", "старый");
        using (var input = File.OpenRead(source))
        using (var decoded = SkiaImageCodec.Load(input))
        {
            using (var readOnly = File.OpenRead(target)) Assert.Throws<NotSupportedException>(() => decoded.Save(readOnly, false));
            Assert.Equal("старый", File.ReadAllText(target));
            using var output = File.Create(target);
            decoded.Save(output, false, 80, 6, 3);
        }
        using (var output = File.OpenRead(target))
        using (var codec = SKCodec.Create(output)) Assert.Equal(6, codec.Info.Width);
        using (new FileStream(target, FileMode.Open, FileAccess.ReadWrite, FileShare.None)) { }
    }

    [Fact]
    public void DisposedImageRejectsSaveAndRepeatedDisposeReleasesSlotOnlyOnce()
    {
        string source = Jpeg();
        using var input = File.OpenRead(source);
        var decoded = SkiaImageCodec.Load(input);
        decoded.Dispose();
        decoded.Dispose();
        using var output = new MemoryStream();
        Assert.Throws<ObjectDisposedException>(() => decoded.Save(output, false));
        input.Position = 0;
        using var next = SkiaImageCodec.Load(input);
        Assert.Equal(12, next.Height);
    }

    [WindowsFact]
    public void LockedOutputDoesNotDamageExistingFile()
    {
        string source = Jpeg();
        string target = Write("result.jpg", "старый");
        using var input = File.OpenRead(source);
        using var decoded = SkiaImageCodec.Load(input);
        using (var held = new FileStream(target, FileMode.Open, FileAccess.Read, FileShare.Read))
            Assert.Throws<IOException>(() => { using var output = File.Create(target); decoded.Save(output, false); });
        Assert.Equal("старый", File.ReadAllText(target));
    }
}
