using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Treynessen.Extensions;
using Treynessen.Translit;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace StoreCMS.ComponentTests;

public abstract class FileTest : IDisposable
{
    private readonly (FieldInfo Field, object? Value)[] cachedPaths;
    protected string Root { get; }
    protected TestEnvironment Env { get; }
    protected ServiceProvider Services { get; }
    protected DefaultHttpContext Context { get; }

    protected FileTest()
    {
        Root = Directory.CreateTempSubdirectory("StoreCMS-T012-").FullName;
        cachedPaths = typeof(Treynessen.Extensions.HostingEnvironmentExtensions).GetFields(BindingFlags.Static | BindingFlags.NonPublic)
            .Where(f => !f.IsInitOnly && (f.FieldType == typeof(string) || f.FieldType == typeof(Treynessen.Extensions.DirectoryInfo[])))
            .Select(f => (f, f.GetValue(null))).ToArray();
        foreach (var entry in cachedPaths) entry.Field.SetValue(null, null);
        Env = new TestEnvironment(Root);
        Directory.CreateDirectory(Env.GetStorageFolderFullPath());
        Services = new ServiceCollection().AddSingleton<IWebHostEnvironment>(Env)
            .AddSingleton<EnRuTranslit>().BuildServiceProvider();
        Context = new DefaultHttpContext { RequestServices = Services };
    }

    protected string Write(string relative, string content = "старое")
    {
        string path = Path.GetFullPath(Path.Combine(Root, relative));
        Assert.StartsWith(Root + Path.DirectorySeparatorChar, path);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, content);
        return path;
    }

    protected void NoTemporaryFiles() => Assert.Empty(Directory.GetFiles(Root, "*.tmp", SearchOption.AllDirectories));

    public void Dispose()
    {
        Services.Dispose();
        foreach (var entry in cachedPaths) entry.Field.SetValue(null, entry.Value);
        string target = Path.GetFullPath(Root);
        string parent = Path.GetFullPath(Path.GetTempPath()).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        Assert.StartsWith(parent, target);
        Assert.StartsWith("StoreCMS-T012-", Path.GetFileName(target));
        Assert.DoesNotContain(Directory.EnumerateFileSystemEntries(target, "*", SearchOption.AllDirectories),
            p => (File.GetAttributes(p) & FileAttributes.ReparsePoint) != 0);
        Directory.Delete(target, true);
        Assert.False(Directory.Exists(target));
    }
}

public sealed class TestEnvironment(string root) : IWebHostEnvironment
{
    public string ApplicationName { get; set; } = "StoreCMS";
    public string EnvironmentName { get; set; } = "Development";
    public string ContentRootPath { get; set; } = root.Replace('\\', '/');
    public string WebRootPath { get; set; } = Path.Combine(root, "Storage");
    public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
}

public sealed class WindowsFactAttribute : FactAttribute
{
    public WindowsFactAttribute()
    {
        if (!OperatingSystem.IsWindows()) Skip = "Отказ через Windows FileShare проверяется только на Windows.";
    }
}
