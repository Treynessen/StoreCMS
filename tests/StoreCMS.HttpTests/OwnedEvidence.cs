using System.Text;

namespace StoreCMS.HttpTests;

internal sealed class OwnedEvidence
{
    private readonly string parent;
    private readonly string identity;
    internal string Root { get; }
    internal string RunId { get; }
    internal string Marker => Path.Combine(Root, ".owner");

    internal OwnedEvidence(string allowedParent, string runId)
    {
        if (!Path.IsPathFullyQualified(allowedParent)
            || !Guid.TryParseExact(runId, "N", out _))
            throw new InvalidOperationException("InvalidOwnedRoot");
        parent = Path.TrimEndingDirectorySeparator(Path.GetFullPath(allowedParent));
        string temp = Path.TrimEndingDirectorySeparator(Path.GetFullPath(Path.GetTempPath()));
        string parentName = Path.GetFileName(parent);
        if (!string.Equals(Path.GetDirectoryName(parent), temp, OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal)
            || !parentName.StartsWith("StoreCMS-T014-", StringComparison.Ordinal)
            || !Guid.TryParseExact(parentName["StoreCMS-T014-".Length..], "N", out _))
            throw new InvalidOperationException("UnapprovedParent");
        CheckAncestors(parent);
        RunId = runId;
        Root = Path.Combine(parent, "run-" + RunId);
        if (Path.Exists(Root)) throw new InvalidOperationException("RootAlreadyExists");
        Directory.CreateDirectory(Root);
        identity = RunId + ":" + Guid.NewGuid().ToString("N");
        using var marker = new FileStream(Marker, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        marker.Write(Encoding.UTF8.GetBytes(identity));
    }

    private static void CheckAncestors(string path)
    {
        for (string? current = path; current != null; current = Path.GetDirectoryName(current))
            if (Path.Exists(current) && (File.GetAttributes(current) & FileAttributes.ReparsePoint) != 0)
                throw new InvalidOperationException("LinkedPathForbidden");
    }

    internal void Validate()
    {
        if (!string.Equals(Path.GetDirectoryName(Root), parent, StringComparison.Ordinal)
            || Path.GetFileName(Root) != "run-" + RunId)
            throw new InvalidOperationException("RootIdentityMismatch");
        CheckAncestors(Root);
        if (!Directory.Exists(Root) || !File.Exists(Marker))
            throw new InvalidOperationException("RootIdentityMismatch");
        CheckAncestors(Marker);
        if (!string.Equals(File.ReadAllText(Marker), identity, StringComparison.Ordinal))
            throw new InvalidOperationException("RootIdentityMismatch");
    }

    internal void CreateManifest(string name, string content)
    {
        Validate();
        if (name != Path.GetFileName(name) || name is "" or "." or ".." || name.IndexOfAny(['/', '\\', ':']) >= 0)
            throw new InvalidOperationException("InvalidEvidenceName");
        string path = Path.Combine(Root, name);
        CheckAncestors(path);
        using var file = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        file.Write(Encoding.UTF8.GetBytes(content));
        file.Flush(true);
    }

    private static void CheckTree(string directory)
    {
        CheckAncestors(directory);
        foreach (string entry in Directory.EnumerateFileSystemEntries(directory))
        {
            var attributes = File.GetAttributes(entry);
            if ((attributes & FileAttributes.ReparsePoint) != 0)
                throw new InvalidOperationException("LinkedPathForbidden");
            if ((attributes & FileAttributes.Directory) != 0) CheckTree(entry);
        }
    }

    internal void DeleteOwnedRoot()
    {
        Validate();
        CheckTree(Root);
        Validate();
        Directory.Delete(Root, true);
    }
}