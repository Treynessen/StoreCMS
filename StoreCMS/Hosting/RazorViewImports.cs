using System;
using System.IO;
using System.Text;

namespace Treynessen.Hosting;

public static class RazorViewImports
{
    public const string Content = "@using Treynessen.Functions;\n"
        + "@using Treynessen.Controllers;\n"
        + "@using Treynessen.PagesManagement;\n"
        + "@using Treynessen.ImagesManagement;\n"
        + "@using Treynessen.Database.Context;\n"
        + "@using Treynessen.Database.Entities;\n"
        + "@addTagHelper Treynessen.TagHelpers.ImageTagHelper, StoreCMS\n"
        + "@addTagHelper Treynessen.TagHelpers.PageButtonsTagHelper, StoreCMS\n";

    public static void Initialize(string contentRoot)
    {
        if (string.IsNullOrWhiteSpace(contentRoot) || !Path.IsPathFullyQualified(contentRoot))
            throw new ArgumentException("ContentRoot должен быть абсолютным путём.", nameof(contentRoot));
        string root = Path.GetFullPath(contentRoot);
        ValidatePath(root);
        if (!Directory.Exists(root)) throw new DirectoryNotFoundException(root);
        foreach (string folder in new[] { "Templates", "Chunks" })
            ValidatePath(Path.Combine(root, "Views", folder, "_ViewImports.cshtml"));
        foreach (string folder in new[] { "Templates", "Chunks" })
            EnsureFile(Path.Combine(root, "Views", folder));
    }

    private static void EnsureFile(string directory)
    {
        string target = Path.Combine(directory, "_ViewImports.cshtml");
        ValidatePath(target);
        if (IsExistingFile(target)) return;
        string views = Path.GetDirectoryName(directory);
        Directory.CreateDirectory(views);
        ValidatePath(views);
        Directory.CreateDirectory(directory);
        ValidatePath(directory);
        string temporary = Path.Combine(directory, ".viewimports-" + Guid.NewGuid().ToString("N") + ".tmp");
        try
        {
            // [Комментарий ИИ-агента]: Публикуем только закрытый полный UTF-8 файл; конкурентный победитель и пользовательский файл сохраняются.
            using (var stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                byte[] bytes = new UTF8Encoding(false, true).GetBytes(Content);
                stream.Write(bytes);
                stream.Flush(true);
            }
            ValidatePath(target);
            try { File.Move(temporary, target, false); }
            catch (IOException) when (IsExistingFile(target)) { }
        }
        finally
        {
            // [Комментарий ИИ-агента]: Ошибки очистки не скрываем; после отказа запуск не должен продолжаться.
            File.Delete(temporary);
        }
    }

    private static bool IsExistingFile(string path)
    {
        FileAttributes? attributes = Attributes(path);
        if (!attributes.HasValue) return false;
        if ((attributes.Value & (FileAttributes.Directory | FileAttributes.ReparsePoint)) != 0)
            throw new IOException("Ожидался обычный файл: " + path);
        return true;
    }

    private static void ValidatePath(string path)
    {
        // [Комментарий ИИ-агента]: Проверяем также предков ContentRoot, чтобы не следовать junction/symlink за разрешённый корень.
        for (string current = path; current != null; current = Path.GetDirectoryName(current))
        {
            FileAttributes? attributes = Attributes(current);
            if (attributes.HasValue && (attributes.Value & FileAttributes.ReparsePoint) != 0)
                throw new IOException("Reparse point в пути Razor imports: " + current);
        }
    }

    private static FileAttributes? Attributes(string path)
    {
        try { return File.GetAttributes(path); }
        catch (FileNotFoundException) { return null; }
        catch (DirectoryNotFoundException) { return null; }
    }
}
