using System.Text.Json;
using Treynessen.AdminPanelTypes;
using Treynessen.SettingsManagement;
using Xunit;

namespace StoreCMS.ComponentTests;

[Trait("Category", "FileSystem")]
[Trait("Area", "B03")]
public sealed class ConfigurationFileTests : FileTest
{
    private const string Initial = "{\"Unknown\":{\"keep\":[1,true,\"данные\"]},\"dbsettings\":{\"Database\":\"synthetic\"},\"categorypagesettings\":{\"PaginationStyleName\":\"old\"}}";
    private static SettingsModel Model(string value) => new() { PaginationStyleName = value, AccessSettings = new() };

    [Fact]
    public void SavePreservesUnknownFieldsCasingAndEscapesAndCanRepeat()
    {
        string path = Write("Settings/config.json", Initial);
        var handler = new ConfigurationHandler(path);
        foreach (string text in new[] { "кавычки\"\\\n<script>@", "повтор" })
        {
            handler.ChangeConfigFile(Model(text));
            using var json = JsonDocument.Parse(File.ReadAllText(path));
            Assert.Equal(text, json.RootElement.GetProperty("categorypagesettings").GetProperty("PaginationStyleName").GetString());
            Assert.Equal("данные", json.RootElement.GetProperty("Unknown").GetProperty("keep")[2].GetString());
            Assert.Equal("synthetic", json.RootElement.GetProperty("dbsettings").GetProperty("Database").GetString());
            Assert.Equal(55, json.RootElement.GetProperty("AdminPanelAccessSettings").EnumerateObject().Count());
            NoTemporaryFiles();
        }
    }

    [WindowsFact]
    public void DeniedReplacementPreservesPreviousBytesCleansTemporaryAndAllowsRetry()
    {
        string path = Write("Settings/config.json", Initial);
        var handler = new ConfigurationHandler(path);
        using (var held = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            Assert.Throws<IOException>(() => handler.ChangeConfigFile(Model("новое")));
        Assert.Equal(Initial, File.ReadAllText(path));
        NoTemporaryFiles();
        handler.ChangeConfigFile(Model("повтор"));
        using var saved = JsonDocument.Parse(File.ReadAllText(path));
        Assert.Equal("повтор", saved.RootElement.GetProperty("categorypagesettings").GetProperty("PaginationStyleName").GetString());
        NoTemporaryFiles();
    }

    [Theory]
    [InlineData("[]", typeof(InvalidDataException))]
    [InlineData("{\"CategoryPageSettings\":42}", typeof(InvalidDataException))]
    [InlineData("{bad", typeof(JsonException))]
    public void InvalidDocumentIsNotOverwritten(string invalid, Type expectedError)
    {
        string path = Write("Settings/config.json", Initial);
        var handler = new ConfigurationHandler(path);
        File.WriteAllText(path, invalid);
        var error = Record.Exception(() => handler.ChangeConfigFile(Model("new")));
        Assert.NotNull(error);
        Assert.True(expectedError.IsInstanceOfType(error), error.GetType().FullName);
        Assert.Equal(invalid, File.ReadAllText(path));
        NoTemporaryFiles();
    }

    [Fact]
    public void InvalidModelDoesNotWrite()
    {
        string path = Write("Settings/config.json", Initial);
        var handler = new ConfigurationHandler(path);
        Assert.Throws<ArgumentNullException>(() => handler.ChangeConfigFile(null!));
        Assert.Throws<ArgumentNullException>(() => handler.ChangeConfigFile(new SettingsModel { AccessSettings = null! }));
        Assert.Equal(Initial, File.ReadAllText(path));
        NoTemporaryFiles();
    }

    [Fact]
    public void MissingConfigIsRejectedWithoutCreation()
    {
        string path = Path.Combine(Root, "absent.json");
        Assert.Throws<FileNotFoundException>(() => new ConfigurationHandler(path));
        Assert.False(File.Exists(path));
    }
}
