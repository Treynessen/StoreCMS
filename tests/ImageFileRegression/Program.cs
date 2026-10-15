using System;
using System.IO;
using System.Linq;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;
using Treynessen.Functions;
using Treynessen.ImagesManagement;

internal class Program
{
    private static int checks;
    private static readonly string[] Suffixes = { "", "_320x240", "_q75", "_320x240_q100" };

    private static void Main()
    {
        // Each run owns a unique temporary directory; no application or real DB starts.
        string root = Path.Combine(AppContext.BaseDirectory, "scratch", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            string[] names = { "a", "dreli-shurupoverty", "dreli-shurupoverty_2", "dreli_12", "a.b+(x)[1]" };
            foreach (string name in names)
            {
                CheckDelete(root, name, ".jpg", false);
                CheckRename(root, name, ".jpg", false);
            }
            CheckDelete(root, "photo", ".png", true);
            CheckRename(root, "photo_2", ".jpeg", true);
            CheckDelete(root, "photo", ".j+g", false);
            CheckRename(root, "photo", ".j+g", false);
            CheckAbsentOriginal(root);
            CheckSeparateProducts(root);
            Console.WriteLine("PASS: " + checks + " regression cases; original methods linked directly, no database connection.");
        }
        finally { Directory.Delete(root, true); }
    }

    private static string CaseFolder(string root) => Directory.CreateDirectory(Path.Combine(root, Guid.NewGuid().ToString("N"))).FullName;
    private static void Put(string folder, string name) => File.WriteAllText(Path.Combine(folder, name), name);
    private static string[] Family(string name, string extension) => Suffixes.Select(s => name + s + extension).ToArray();
    private static string[] Neighbours(string name, string extension) => new[]
    {
        "b" + name + extension, "b" + name + "_320x240_q75" + extension,
        name + "_2" + extension, name + "_2_320x240" + extension,
        name + "_q1000" + extension, name + "_q75_320x240" + extension,
        name + "_320x240_extra" + extension, name + "_320x240" + extension + ".bak",
        name + "_320x240X" + extension.Substring(1), name + "_320x240" + (extension == ".jpeg" ? ".png" : ".jpeg")
    };
    private static void Assert(bool condition, string message) { if (!condition) throw new Exception(message); }
    private static void ExpectFiles(string folder, string[] expected)
    {
        string[] actual = Directory.GetFiles(folder).Select(Path.GetFileName).OrderBy(n => n, StringComparer.Ordinal).ToArray();
        Assert(actual.SequenceEqual(expected.OrderBy(n => n, StringComparer.Ordinal)), "Unexpected file set in " + folder);
        foreach (string file in actual) Assert(File.ReadAllText(Path.Combine(folder, file)).Length > 0, "File content lost");
    }
    private static void CheckDelete(string root, string name, string extension, bool trailingSlash)
    {
        string folder = CaseFolder(root);
        string[] family = Family(name, extension);
        string[] neighbours = Neighbours(name, extension);
        foreach (string file in family.Concat(neighbours)) Put(folder, file);
        ImagesManagementFunctions.DeleteDependentImages(folder + (trailingSlash ? "/" : ""), family[0]);
        ExpectFiles(folder, new[] { family[0] }.Concat(neighbours).ToArray());
        checks++;
    }
    private static void CheckRename(string root, string name, string extension, bool trailingSlash)
    {
        string folder = CaseFolder(root);
        string[] family = Family(name, extension);
        string[] neighbours = Neighbours(name, extension);
        foreach (string file in family.Concat(neighbours)) Put(folder, file);
        var db = new CMSDatabase();
        var env = new TestEnvironment { StoragePath = root + "/" };
        string shortFolder = "/" + Path.GetFileName(folder) + "/";
        var image = new Image { ShortPath = shortFolder + name + extension, ShortPathHash = OtherFunctions.GetHashFromString(shortFolder + name + extension), FullName = name + extension };
        db.Images.Add(image);
        bool save = trailingSlash;
        ImagesManagementFunctions.RenameImageAndDependencies(db, env, folder + (trailingSlash ? "/" : ""), name, "renamed_3", extension, save);
        string[] renamed = Family("renamed_3", extension);
        ExpectFiles(folder, renamed.Concat(neighbours).ToArray());
        for (int i = 0; i < renamed.Length; i++) Assert(File.ReadAllText(Path.Combine(folder, renamed[i])) == family[i], "Wrong renamed content");
        Assert(image.ShortPath == shortFolder + "renamed_3" + extension && image.FullName == "renamed_3" + extension, "Image metadata changed incorrectly");
        Assert(image.ShortPathHash == OtherFunctions.GetHashFromString(image.ShortPath), "Wrong path hash");
        Assert(db.Saves == (save ? 1 : 0), "Save flag ignored");
        checks++;
    }
    private static void CheckAbsentOriginal(string root)
    {
        string folder = CaseFolder(root);
        Put(folder, "a_320x240.jpg"); Put(folder, "ba.jpg");
        ImagesManagementFunctions.DeleteDependentImages(folder, "a.jpg");
        ExpectFiles(folder, new[] { "ba.jpg" });
        checks++;
    }
    private static void CheckSeparateProducts(string root)
    {
        string products = Path.Combine(CaseFolder(root), "images", "products");
        string first = Directory.CreateDirectory(Path.Combine(products, "41")).FullName;
        string second = Directory.CreateDirectory(Path.Combine(products, "42")).FullName;
        foreach (string file in Family("dreli_2", ".jpg")) { Put(first, file); Put(second, file); }
        ImagesManagementFunctions.DeleteDependentImages(first, "dreli_2.jpg");
        ExpectFiles(first, new[] { "dreli_2.jpg" }); ExpectFiles(second, Family("dreli_2", ".jpg"));
        ImagesManagementFunctions.RenameImageAndDependencies(new CMSDatabase(), new TestEnvironment { StoragePath = root + "/" }, first, "dreli_2", "dreli", ".jpg", false);
        ExpectFiles(first, new[] { "dreli.jpg" }); ExpectFiles(second, Family("dreli_2", ".jpg"));
        checks++;
    }
    private class TestEnvironment : Microsoft.AspNetCore.Hosting.IHostingEnvironment { public string StoragePath { get; set; } }
}
