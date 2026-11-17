using System.Diagnostics;
using StoreCMS.UnitTests.TestDoubles;
using Xunit;

namespace StoreCMS.HttpTests;

internal sealed class SyntheticArea : IDisposable
{
    internal string Parent { get; } = Path.Combine(Path.GetTempPath(), "StoreCMS-T014-" + Guid.NewGuid().ToString("N"));
    internal string Sentinel => Path.Combine(Parent, "sentinel.txt");
    internal SyntheticArea()
    {
        Directory.CreateDirectory(Parent);
        File.WriteAllText(Sentinel, "соседний синтетический файл");
    }

    internal OwnedEvidence Create() => new(Parent, Guid.NewGuid().ToString("N"));

    private static void DeleteWithoutFollowingLinks(string path)
    {
        var attributes = File.GetAttributes(path);
        if ((attributes & FileAttributes.ReparsePoint) != 0)
        {
            if ((attributes & FileAttributes.Directory) != 0) Directory.Delete(path);
            else File.Delete(path);
            return;
        }
        if ((attributes & FileAttributes.Directory) != 0)
        {
            foreach (string child in Directory.EnumerateFileSystemEntries(path))
                DeleteWithoutFollowingLinks(child);
            Directory.Delete(path);
        }
        else File.Delete(path);
    }

    public void Dispose()
    {
        Assert.True(File.Exists(Sentinel), "SiblingSentinelMissing");
        Assert.True(File.ReadAllText(Sentinel) == "соседний синтетический файл", "SiblingSentinelChanged");
        Assert.True(Path.GetDirectoryName(Path.GetFullPath(Parent)) ==
            Path.TrimEndingDirectorySeparator(Path.GetFullPath(Path.GetTempPath())));
        Assert.StartsWith("StoreCMS-T014-", Path.GetFileName(Parent), StringComparison.Ordinal);
        DeleteWithoutFollowingLinks(Parent);
        Assert.False(Path.Exists(Parent));
    }

    internal static void LinkDirectory(string link, string target)
    {
        if (!OperatingSystem.IsWindows())
        {
            Directory.CreateSymbolicLink(link, target);
            return;
        }
        var start = new ProcessStartInfo("cmd.exe") { UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true };
        start.ArgumentList.Add("/d");
        start.ArgumentList.Add("/c");
        start.ArgumentList.Add("mklink");
        start.ArgumentList.Add("/J");
        start.ArgumentList.Add(link);
        start.ArgumentList.Add(target);
        using var process = Process.Start(start) ?? throw new InvalidOperationException("SyntheticJunctionUnavailable");
        Task<string> output = process.StandardOutput.ReadToEndAsync();
        Task<string> error = process.StandardError.ReadToEndAsync();
        if (!process.WaitForExit(5000)) throw new InvalidOperationException("SyntheticJunctionTimeout");
        Task.WaitAll(output, error);
        if (process.ExitCode != 0) throw new InvalidOperationException("SyntheticJunctionUnavailable");
    }
}

[Trait("Category", "OfflineHttp")]
public sealed class OwnedEvidenceTests : NetworkIsolatedTest
{
    [Fact]
    public void ManifestUsesCreateNewAndCleanupPreservesSibling()
    {
        using var area = new SyntheticArea();
        var root = area.Create();
        root.CreateManifest("manifest.json", "{}");
        Assert.Throws<IOException>(() => root.CreateManifest("manifest.json", "replacement"));
        Assert.True(File.ReadAllText(Path.Combine(root.Root, "manifest.json")) == "{}");
        root.DeleteOwnedRoot();
        Assert.False(Directory.Exists(root.Root));
        Assert.True(File.Exists(area.Sentinel));
    }

    [Fact]
    public void DuplicateRunCannotAdoptExistingRoot()
    {
        using var area = new SyntheticArea();
        var root = area.Create();
        Assert.Throws<InvalidOperationException>(() => new OwnedEvidence(area.Parent, root.RunId));
        root.Validate();
    }

    [Theory]
    [InlineData("../sentinel.txt")]
    [InlineData("nested/manifest.json")]
    [InlineData("nested\\manifest.json")]
    [InlineData("C:\\foreign.json")]
    [InlineData("")]
    [InlineData("..")]
    public void ForeignManifestPathIsRejectedWithoutChangingSentinel(string name)
    {
        using var area = new SyntheticArea();
        var root = area.Create();
        Assert.Throws<InvalidOperationException>(() => root.CreateManifest(name, "rejected"));
        root.DeleteOwnedRoot();
    }

    [Fact]
    public void RelativeOrForeignParentAndInvalidRunIdAreRejectedBeforeWrites()
    {
        using var area = new SyntheticArea();
        Assert.Throws<InvalidOperationException>(() => new OwnedEvidence("relative-root", Guid.NewGuid().ToString("N")));
        Assert.Throws<InvalidOperationException>(() => new OwnedEvidence(Path.GetTempPath(), Guid.NewGuid().ToString("N")));
        Assert.Throws<InvalidOperationException>(() => new OwnedEvidence(area.Parent, "not-a-run"));
        Assert.Single(Directory.GetFileSystemEntries(area.Parent));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MissingOrChangedMarkerPreventsWritesAndDeletion(bool missing)
    {
        using var area = new SyntheticArea();
        var root = area.Create();
        if (missing) File.Delete(root.Marker);
        else File.WriteAllText(root.Marker, "foreign-marker");
        Assert.Throws<InvalidOperationException>(root.Validate);
        Assert.Throws<InvalidOperationException>(() => root.CreateManifest("manifest.json", "{}"));
        Assert.Throws<InvalidOperationException>(root.DeleteOwnedRoot);
        Assert.True(Directory.Exists(root.Root));
    }

    [Fact]
    public void NestedLinkIsRejectedBeforeTraversalAndTargetSurvives()
    {
        using var area = new SyntheticArea();
        var root = area.Create();
        string target = Path.Combine(area.Parent, "target");
        Directory.CreateDirectory(target);
        File.WriteAllText(Path.Combine(target, "keep.txt"), "keep");
        string link = Path.Combine(root.Root, "link");
        SyntheticArea.LinkDirectory(link, target);
        Assert.Throws<InvalidOperationException>(root.DeleteOwnedRoot);
        Assert.True(File.Exists(Path.Combine(target, "keep.txt")));
        Assert.True(File.Exists(root.Marker));
        Directory.Delete(link);
        root.DeleteOwnedRoot();
        Assert.True(File.Exists(Path.Combine(target, "keep.txt")));
    }

    [Fact]
    public void LinkedRootCannotBeAdoptedOrDeleted()
    {
        using var area = new SyntheticArea();
        var root = area.Create();
        string target = Path.Combine(area.Parent, "replacement");
        Directory.CreateDirectory(target);
        File.WriteAllText(Path.Combine(target, "keep.txt"), "keep");
        Directory.Delete(root.Root, true);
        SyntheticArea.LinkDirectory(root.Root, target);
        Assert.Throws<InvalidOperationException>(root.Validate);
        Assert.Throws<InvalidOperationException>(root.DeleteOwnedRoot);
        Assert.True(File.Exists(Path.Combine(target, "keep.txt")));
        Directory.Delete(root.Root);
    }

    [Fact]
    public void LinkedParentIsRejectedBeforeCreatingRun()
    {
        using var area = new SyntheticArea();
        string target = Path.Combine(area.Parent, "target");
        Directory.CreateDirectory(target);
        string link = Path.Combine(Path.GetTempPath(), "StoreCMS-T014-" + Guid.NewGuid().ToString("N"));
        try
        {
            SyntheticArea.LinkDirectory(link, target);
            Assert.Throws<InvalidOperationException>(() => new OwnedEvidence(link, Guid.NewGuid().ToString("N")));
            Assert.Empty(Directory.GetFileSystemEntries(target));
        }
        finally
        {
            if (Directory.Exists(link)) Directory.Delete(link);
        }
    }
}