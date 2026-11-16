using Treynessen.Controllers;
using Xunit;

namespace StoreCMS.ComponentTests;

[Trait("Category", "FileSystem")]
[Trait("Area", "B10")]
public sealed class ProductBlockTests : FileTest
{
    [Fact]
    public void PublishesBothAndExplicitSaveReplacesBoth()
    {
        string source = Write("Settings/product.txt", "S0");
        string view = Write("Views/product.cshtml", "V0");
        AdminPanelController.PublishProductBlock(source, view, "S1", "V1");
        Assert.Equal("S1", File.ReadAllText(source));
        Assert.Equal("V1", File.ReadAllText(view));
        AdminPanelController.PublishProductBlock(source, view, "S2", "V2");
        Assert.Equal("S2", File.ReadAllText(source));
        Assert.Equal("V2", File.ReadAllText(view));
        NoTemporaryFiles();
    }

    [WindowsFact]
    public void FirstFileFailurePreservesBothAndDoesNotReportPartialPublication()
    {
        string source = Write("source.txt", "S0");
        string view = Write("view.cshtml", "V0");
        using (var held = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        {
            var error = Assert.Throws<IOException>(() => AdminPanelController.PublishProductBlock(source, view, "S1", "V1"));
            Assert.DoesNotContain("PartiallyPublished", error.Message);
        }
        Assert.Equal("S0", File.ReadAllText(source));
        Assert.Equal("V0", File.ReadAllText(view));
        NoTemporaryFiles();
    }

    [WindowsFact]
    public void SecondFileFailureKeepsNewSourceOldViewAndExplicitSaveRecovers()
    {
        string source = Write("source.txt", "S0");
        string view = Write("view.cshtml", "V0");
        using (var held = new FileStream(view, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        {
            var error = Assert.Throws<IOException>(() => AdminPanelController.PublishProductBlock(source, view, "S1", "V1"));
            Assert.Contains("StoreCMS.ProductBlock.PartiallyPublished", error.Message);
            Assert.IsType<IOException>(error.InnerException);
            Assert.Equal("S1", File.ReadAllText(source));
            Assert.Equal("V0", File.ReadAllText(view));
            NoTemporaryFiles();
        }
        AdminPanelController.PublishProductBlock(source, view, "S1", "V1");
        Assert.Equal("S1", File.ReadAllText(source));
        Assert.Equal("V1", File.ReadAllText(view));
        NoTemporaryFiles();
    }

    [Fact]
    public void SecondTargetDirectoryProducesPartialPublicationWithoutDeletingSentinel()
    {
        string source = Write("source.txt", "S0");
        string sentinel = Write("view.cshtml/sentinel", "keep");
        var error = Assert.Throws<IOException>(() => AdminPanelController.PublishProductBlock(source, Path.GetDirectoryName(sentinel)!, "S1", "V1"));
        Assert.Contains("PartiallyPublished", error.Message);
        Assert.Equal("S1", File.ReadAllText(source));
        Assert.Equal("keep", File.ReadAllText(sentinel));
        NoTemporaryFiles();
    }
}
