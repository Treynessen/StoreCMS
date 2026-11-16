using System.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Treynessen.TemplatesManagement;
using Xunit;

namespace StoreCMS.ComponentTests;

[Trait("Category", "FileSystem")]
[Trait("Area", "Razor")]
public sealed class RazorTests : FileTest
{
    [Fact]
    public void ExistingProductionViewsAreCompiledAndDiscoveredAsRazorPageTypes()
    {
        var manager = new ServiceCollection().AddLogging().AddControllersWithViews()
            .AddApplicationPart(typeof(TemplatesManagementFunctions).Assembly).PartManager;
        var feature = new ViewsFeature();
        manager.PopulateFeature(feature);
        Assert.NotEmpty(feature.ViewDescriptors);
        Assert.Contains(feature.ViewDescriptors, v => v.RelativePath == "/Views/AdminPanel/LoginForm.cshtml");
        Assert.Contains(feature.ViewDescriptors, v => v.RelativePath == "/Views/AdminPanel/Settings.cshtml");
        Assert.All(feature.ViewDescriptors, v => Assert.True(typeof(IRazorPage).IsAssignableFrom(v.Type)));
        Assert.Equal(feature.ViewDescriptors.Count, feature.ViewDescriptors.Select(v => v.RelativePath).Distinct().Count());
    }

    private ServiceProvider RazorServices()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        var physical = new PhysicalFileProvider(Root);
        services.AddSingleton(physical);
        services.AddSingleton<IWebHostEnvironment>(Env);
        services.AddSingleton<IHostEnvironment>(Env);
        var diagnostic = new DiagnosticListener("StoreCMS.ComponentTests");
        services.AddSingleton<DiagnosticListener>(diagnostic);
        services.AddSingleton<DiagnosticSource>(diagnostic);
        services.AddControllersWithViews().AddApplicationPart(typeof(TemplatesManagementFunctions).Assembly)
            .AddRazorRuntimeCompilation(options =>
            {
                options.FileProviders.Clear();
                options.FileProviders.Add(new GeneratedRazorFileProvider(physical));
            });
        return services.BuildServiceProvider();
    }

    [Fact]
    public async Task GeneratedSourceAndChunkCompileWithRealRazorWithoutHost()
    {
        string generated = TemplatesManagementFunctions.SourceToCSHTML(null!, "<p>mail@example.org [Page:Title] [#self]</p>",
            "Treynessen.Database.Entities.UsualPage", Env, "self");
        TemplatesManagementFunctions.WriteCshtmlContentToFile(Path.Combine(Root, "Views/Templates"), "sample", generated);
        Write("Views/Chunks/sample.cshtml", "<span>@(2 + 3)</span>");
        using var services = RazorServices();
        var compiler = services.GetRequiredService<IViewCompilerProvider>().GetCompiler();
        foreach (string path in new[] { "/Views/Templates/sample.cshtml", "/Views/Chunks/sample.cshtml" })
        {
            var compiled = await compiler.CompileAsync(path);
            Assert.NotNull(compiled.Type);
            Assert.True(typeof(IRazorPage).IsAssignableFrom(compiled.Type));
        }
    }

    [Fact]
    public async Task DamagedRazorRejectedAndExplicitSaveWithNewCompilerRecovers()
    {
        string path = Write("Views/Templates/broken.cshtml", "@{ int x = ; }");
        using (var services = RazorServices())
        {
            var compiler = services.GetRequiredService<IViewCompilerProvider>().GetCompiler();
            var error = await Assert.ThrowsAnyAsync<Exception>(() => compiler.CompileAsync("/Views/Templates/broken.cshtml"));
            Assert.Contains("CompilationFailedException", error.GetType().Name);
        }
        TemplatesManagementFunctions.WriteCshtmlContentToFile(Path.GetDirectoryName(path)!, "broken", "<p>@(6 * 7)</p>");
        using var recovered = RazorServices();
        Assert.NotNull((await recovered.GetRequiredService<IViewCompilerProvider>().GetCompiler().CompileAsync("/Views/Templates/broken.cshtml")).Type);
    }

    [Theory]
    [InlineData("../outside.cshtml")]
    [InlineData("/../outside.cshtml")]
    [InlineData("/Views/Templates/../../../../outside.cshtml")]
    [InlineData("/Views/Templates/bad\0.cshtml")]
    public void ProviderRejectsInvalidOrEscapingPaths(string path)
    {
        Write("outside.cshtml", "keep");
        using var physical = new PhysicalFileProvider(Root);
        var provider = new GeneratedRazorFileProvider(physical);
        Assert.False(provider.GetFileInfo(path).Exists);
        Assert.Equal("keep", File.ReadAllText(Path.Combine(Root, "outside.cshtml")));
    }

    [Theory]
    [InlineData("Templates")]
    [InlineData("Chunks")]
    public void GeneratedProviderReaderSurvivesRenameAndNewPublication(string folder)
    {
        string path = Write($"Views/{folder}/sample.cshtml", "OLD");
        using var physical = new PhysicalFileProvider(Root);
        var provider = new GeneratedRazorFileProvider(physical);
        var info = provider.GetFileInfo($"/Views/{folder}/sample.cshtml");
        Assert.Equal("GeneratedRazorFileInfo", info.GetType().Name);
        using (var reader = new StreamReader(info.CreateReadStream()))
        {
            File.Move(path, path + ".old");
            TemplatesManagementFunctions.WriteCshtmlContentToFile(Path.GetDirectoryName(path)!, "sample", "NEW");
            Assert.Equal("OLD", reader.ReadToEnd());
            Assert.Equal("NEW", File.ReadAllText(path));
        }
        NoTemporaryFiles();
    }

    [WindowsFact]
    public void LongLivedGeneratedReaderCanRefuseReplacementWithoutDamageAndExplicitRetrySucceeds()
    {
        string path = Write("Views/Templates/reader.cshtml", "OLD");
        using var physical = new PhysicalFileProvider(Root);
        var provider = new GeneratedRazorFileProvider(physical);
        using (var reader = provider.GetFileInfo("/Views/Templates/reader.cshtml").CreateReadStream())
        {
            Assert.Throws<IOException>(() => TemplatesManagementFunctions.WriteCshtmlContentToFile(Path.GetDirectoryName(path)!, "reader", "NEW"));
            Assert.Equal("OLD", File.ReadAllText(path));
            NoTemporaryFiles();
        }
        TemplatesManagementFunctions.WriteCshtmlContentToFile(Path.GetDirectoryName(path)!, "reader", "NEW");
        Assert.Equal("NEW", File.ReadAllText(path));
        NoTemporaryFiles();
    }
    [Fact]
    public void WriterRejectsInvalidNameWithoutTouchingPreviousFile()
    {
        string path = Write("Views/Templates/good.cshtml", "OLD");
        Assert.Throws<ArgumentException>(() => TemplatesManagementFunctions.WriteCshtmlContentToFile(Path.GetDirectoryName(path)!, "bad\0", "NEW"));
        Assert.Equal("OLD", File.ReadAllText(path));
        NoTemporaryFiles();
    }

    [Fact]
    public void PrePublicationFailurePreservesPreviousFileAndCleansOnlyOwnTemporary()
    {
        string path = Write("Views/Templates/good.cshtml", "OLD");
        string sentinel = Write("Views/Templates/.unrelated.tmp", "keep");
        Assert.Throws<IOException>(() => TemplatesManagementFunctions.WriteCshtmlContentToFile(Path.GetDirectoryName(path)!, "good", "NEW",
            temporary => { Assert.Equal("NEW", File.ReadAllText(temporary)); throw new IOException("Синтетический отказ перед публикацией."); }));
        Assert.Equal("OLD", File.ReadAllText(path));
        Assert.Equal("keep", File.ReadAllText(sentinel));
        Assert.Equal(new[] { sentinel }, Directory.GetFiles(Root, "*.tmp", SearchOption.AllDirectories));
        TemplatesManagementFunctions.WriteCshtmlContentToFile(Path.GetDirectoryName(path)!, "good", "RECOVERED");
        Assert.Equal("RECOVERED", File.ReadAllText(path));
    }
}
