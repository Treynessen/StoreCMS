using Treynessen.Extensions;
using Treynessen.ImagesManagement;
using Xunit;

namespace StoreCMS.ComponentTests;

[Trait("Category", "FileSystem")]
[Trait("Area", "B09")]
public sealed class ProductDeletionTests : FileTest
{
    private string Product(int id) => Write($"Storage/images/products/{id}/image.jpg", "image-" + id);
    private string[] Staged() => Directory.Exists(Path.Combine(Root, "App_Data/product-deletions"))
        ? Directory.GetFiles(Path.Combine(Root, "App_Data/product-deletions"), "*", SearchOption.AllDirectories) : [];

    [Fact]
    public void ConfirmedDeletionCleansOnlyStagedProductsAndCanRepeat()
    {
        string first = Product(41), second = Product(42), unrelated = Product(410);
        using (var files = new ProductDeletionFiles(Env))
        {
            files.Stage(41);
            files.Stage(42);
            Assert.False(File.Exists(first));
            Assert.False(File.Exists(second));
            Assert.Equal(2, Staged().Length);
            files.DeleteCommitted();
            files.DeleteCommitted();
        }
        Assert.Empty(Staged());
        Assert.Equal("image-410", File.ReadAllText(unrelated));
        using var repeat = new ProductDeletionFiles(Env);
        repeat.Stage(41);
        repeat.DeleteCommitted();
        Assert.Empty(Staged());
    }

    [Fact]
    public void ExplicitRestoreAfterKnownFailurePreservesFilesAndAllowsNewAttempt()
    {
        string path = Product(41);
        using (var files = new ProductDeletionFiles(Env))
        {
            files.Stage(41);
            files.Restore(new IOException("Модель подтверждённого SQL rollback, SQL не исполняется."));
        }
        Assert.Equal("image-41", File.ReadAllText(path));
        Assert.Empty(Staged());
        using var retry = new ProductDeletionFiles(Env);
        retry.Stage(41);
        retry.DeleteCommitted();
        Assert.False(File.Exists(path));
    }

    [Fact]
    public void DisposeDoesNotCompensateUnknownCommit()
    {
        string path = Product(41);
        using (var files = new ProductDeletionFiles(Env)) files.Stage(41);
        Assert.False(File.Exists(path));
        Assert.Equal("image-41", File.ReadAllText(Assert.Single(Staged())));
    }

    [Fact]
    public void StagingFailureDoesNotDeleteSourceOrUnrelatedFiles()
    {
        string source = Product(41);
        string unrelated = Product(42);
        Write("App_Data/product-deletions", "препятствие");
        using var files = new ProductDeletionFiles(Env);
        Assert.Throws<IOException>(() => files.Stage(41));
        Assert.Equal("image-41", File.ReadAllText(source));
        Assert.Equal("image-42", File.ReadAllText(unrelated));
    }

    [Fact]
    public void RestoreCollisionRetainsStagedDataAndReportsAggregateFailure()
    {
        string source = Product(41);
        using var files = new ProductDeletionFiles(Env);
        files.Stage(41);
        Write("Storage/images/products/41/new.jpg", "новый");
        var failure = new IOException("Подтверждённый синтетический отказ.");
        var error = Assert.Throws<AggregateException>(() => files.Restore(failure));
        Assert.Same(failure, error.InnerExceptions[0]);
        Assert.Equal(2, error.InnerExceptions.Count);
        Assert.Equal("image-41", File.ReadAllText(Assert.Single(Staged())));
        Assert.False(File.Exists(source));
        Assert.Equal("новый", File.ReadAllText(Path.Combine(Path.GetDirectoryName(source)!, "new.jpg")));
    }

    [WindowsFact]
    public void CleanupFailureRetainsStagedDataAndExplicitCleanupCanRecover()
    {
        Product(41);
        using var files = new ProductDeletionFiles(Env);
        files.Stage(41);
        string staged = Assert.Single(Staged());
        using (var held = new FileStream(staged, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            var error = Assert.Throws<IOException>(() => files.DeleteCommitted());
            Assert.Contains("Не повторяйте удаление вслепую", error.Message);
            Assert.True(File.Exists(staged));
        }
        files.DeleteCommitted();
        Assert.Empty(Staged());
    }
}
