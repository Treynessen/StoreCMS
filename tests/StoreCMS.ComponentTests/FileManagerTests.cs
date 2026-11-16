using Microsoft.AspNetCore.Http;
using Treynessen.AdminPanelTypes;
using Treynessen.Extensions;
using Treynessen.FileManagerManagement;
using Xunit;

namespace StoreCMS.ComponentTests;

[Trait("Category", "FileSystem")]
[Trait("Area", "B04")]
public sealed class FileManagerTests : FileTest
{
    [Theory]
    [InlineData("css")]
    [InlineData("js")]
    public void CreateReadEditRenameAndDelete(string extension)
    {
        if (extension == "css") FileManagerManagementFunctions.CreateCssFile(null!, "", "sample", Context, out _);
        else FileManagerManagementFunctions.CreateScriptFile(null!, "", "sample", Context, out _);
        string original = Path.Combine(Env.GetStorageFolderFullPath(), "sample." + extension);
        Assert.Equal("", File.ReadAllText(original));
        Edit("sample." + extension, new StyleModel { FileName = "sample", FileContent = "первое" }, extension, out bool saved);
        Assert.True(saved);
        Assert.Equal("первое", File.ReadAllText(original));
        Edit("sample." + extension, new StyleModel { FileName = "renamed", FileContent = "второе" }, extension, out saved);
        Assert.True(saved);
        Assert.False(File.Exists(original));
        string renamed = Path.Combine(Env.GetStorageFolderFullPath(), "renamed." + extension);
        Assert.Equal("второе", File.ReadAllText(renamed));
        FileManagerManagementFunctions.DeleteFileOrFolder(null!, "renamed." + extension, Context, out string redirect);
        Assert.NotNull(redirect);
        Assert.False(File.Exists(renamed));
        NoTemporaryFiles();
    }

    private void Edit(string path, StyleModel model, string extension, out bool saved)
    {
        if (extension == "css") FileManagerManagementFunctions.EditCssFile(null!, path, model, Context, out _, out saved);
        else FileManagerManagementFunctions.EditScriptFile(null!, path, model, Context, out _, out saved);
    }

    [Fact]
    public void UploadCollisionPreservesExistingFileAndPublishesCompleteNewFile()
    {
        string existing = Write("Storage/sample.css", "старый");
        using var input = new MemoryStream("body { color: red; }"u8.ToArray());
        var file = new FormFile(input, 0, input.Length, "file", "sample.css");
        FileManagerManagementFunctions.UploadFileToServer(null!, "", file, Context, out bool saved);
        Assert.True(saved);
        Assert.Equal("старый", File.ReadAllText(existing));
        Assert.Equal("body { color: red; }", File.ReadAllText(Path.Combine(Env.GetStorageFolderFullPath(), "sample_2.css")));
        NoTemporaryFiles();
    }

    [Fact]
    public void CopyFailureCleansOwnTemporaryAndPreservesUnrelatedTemporary()
    {
        string sentinel = Write("Storage/.unrelated.tmp", "не трогать");
        string previous = Write("Storage/sample.css", "старый");
        using var input = new FailingReadStream();
        var file = new FormFile(input, 0, 100, "file", "sample.css");
        Assert.Throws<IOException>(() => FileManagerManagementFunctions.UploadFileToServer(null!, "", file, Context, out _));
        Assert.Equal("старый", File.ReadAllText(previous));
        Assert.Equal("не трогать", File.ReadAllText(sentinel));
        Assert.Equal(new[] { sentinel }, Directory.GetFiles(Env.GetStorageFolderFullPath(), "*.tmp").Select(Path.GetFullPath));
    }

    [Theory]
    [InlineData("../escape")]
    [InlineData("x/../../escape")]
    [InlineData("C:\\escape")]
    [InlineData("x>..>escape")]
    public void TraversalRejectedByMutatingEntryPoints(string path)
    {
        string sentinel = Write("outside.css", "не трогать");
        FileManagerManagementFunctions.CreateCssFile(null!, path, "x", Context, out bool created);
        Assert.False(created);
        FileManagerManagementFunctions.EditCssFile(null!, path, new StyleModel { FileName = "x", FileContent = "new" }, Context, out _, out bool edited);
        Assert.False(edited);
        FileManagerManagementFunctions.DeleteFileOrFolder(null!, path, Context, out string redirect);
        Assert.Null(redirect);
        Assert.Equal("не трогать", File.ReadAllText(sentinel));
        Assert.Empty(Directory.GetFiles(Env.GetStorageFolderFullPath(), "*", SearchOption.AllDirectories));
    }

    [Theory]
    [InlineData("payload.exe", "payload")]
    [InlineData("image.avif", "unsupported")]
    [InlineData("noextension", "payload")]
    [InlineData("empty.css", "")]
    public void UnsupportedOrEmptyUploadRejected(string name, string content)
    {
        using var input = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
        FileManagerManagementFunctions.UploadFileToServer(null!, "", new FormFile(input, 0, input.Length, "file", name), Context, out bool saved);
        Assert.False(saved);
        Assert.Empty(Directory.GetFiles(Env.GetStorageFolderFullPath()));
    }

    [Fact]
    public void ProtectedDirectoryAndMissingPathRejected()
    {
        Directory.CreateDirectory(Path.Combine(Env.GetStorageFolderFullPath(), "styles/admin_panel"));
        FileManagerManagementFunctions.CreateCssFile(null!, "styles>admin_panel", "x", Context, out bool saved);
        Assert.False(saved);
        FileManagerManagementFunctions.CreateCssFile(null!, "missing", "x", Context, out saved);
        Assert.False(saved);
        Assert.Empty(Directory.GetFiles(Env.GetStorageFolderFullPath(), "*", SearchOption.AllDirectories));
    }

    [WindowsFact]
    public void DeniedEditAndDeletePreservePreviousFile()
    {
        string path = Write("Storage/sample.css", "старый");
        using (var held = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            Assert.Throws<IOException>(() => Edit("sample.css", new StyleModel { FileName = "sample", FileContent = "new" }, "css", out _));
            Assert.Throws<IOException>(() => FileManagerManagementFunctions.DeleteFileOrFolder(null!, "sample.css", Context, out _));
        }
        Assert.Equal("старый", File.ReadAllText(path));
    }

    [Fact]
    public void RenameCollisionDoesNotDamageEitherFile()
    {
        string a = Write("Storage/a.css", "A");
        string b = Write("Storage/b.css", "B");
        Assert.Throws<IOException>(() => Edit("a.css", new StyleModel { FileName = "b", FileContent = "new" }, "css", out _));
        Assert.Equal("A", File.ReadAllText(a));
        Assert.Equal("B", File.ReadAllText(b));
    }

    [Fact]
    public void ListingFiltersUnsupportedNamesButDoesNotDecodeImages()
    {
        Write("Storage/a.css");
        Write("Storage/bad.jpg", "повреждённый JPEG");
        Write("Storage/unsupported.exe");
        Write("Storage/UPPER.css");
        var files = FileManagerManagementFunctions.GetFilesAndFolders(Env.GetStorageFolderFullPath().TrimEnd('/'), Env, out bool exists);
        Assert.True(exists);
        Assert.Equal(new[] { "a", "bad" }, files.Select(f => f.Name));
    }

    private sealed class FailingReadStream : MemoryStream
    {
        public override int Read(byte[] buffer, int offset, int count) => throw new IOException("Синтетический отказ чтения upload.");
    }
}
