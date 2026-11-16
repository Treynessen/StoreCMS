using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Nodes;

namespace Treynessen.Security.Installation;

internal static class InstallationManifest
{
    internal static readonly IReadOnlyList<string> ApplicationTables = Array.AsReadOnly(new[] {
        "AdminPanelLogs", "CategoryPages", "Chunks", "ConnectedUsers", "Images", "ProductPages", "Redirections",
        "SecurityInstallation", "SynonymsForStrings", "Templates", "UserTypes", "Users", "UsualPages", "VisitedPages", "Visitors"
    });
    private static readonly string Json = ReadResource();
    private static string ReadResource()
    {
        using var resource = typeof(InstallationManifest).Assembly.GetManifestResourceStream("StoreCMS.Installation.E13Manifest.json")
            ?? throw new InvalidOperationException("Отсутствует встроенный контракт E1.3.");
        using var reader = new StreamReader(resource);
        return reader.ReadToEnd();
    }
    // [Комментарий ИИ-агента]: Каждый вызывающий получает копию, чтобы тестовый снимок не мог изменить эталон в памяти.
    internal static JsonObject LoadFacts(bool automatic = false)
    {
        if (!automatic) return JsonNode.Parse(Json)["facts"].AsObject();
        using var stream = typeof(InstallationManifest).Assembly.GetManifestResourceStream("StoreCMS.Installation.E17Manifest.json");
        using var reader = new StreamReader(stream);
        return JsonNode.Parse(reader.ReadToEnd())["facts"].AsObject();
    }
}
