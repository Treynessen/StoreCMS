using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Treynessen.ImagesManagement
{
    public partial class ImageHandler
    {
        private string sourceContentHash;

        private sealed class SourceRevision
        {
            public string Hash { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
        }

        private string RevisionPath()
        {
            string key = Path.GetFullPath(sourceImageFullPath);
            if (OperatingSystem.IsWindows()) key = key.ToUpperInvariant();
            return Path.Combine(env.ContentRootPath, "App_Data", "image-revisions",
                Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(key))) + ".json");
        }

        private SourceRevision ReadRevision()
        {
            string path = RevisionPath();
            if (!File.Exists(path)) return null;
            try
            {
                var revision = JsonSerializer.Deserialize<SourceRevision>(File.ReadAllText(path));
                return revision?.Hash?.Length == 64 && revision.Width > 0 && revision.Height > 0 ? revision : null;
            }
            catch (JsonException) { return null; }
        }

        private void WriteRevision(SourceRevision revision)
        {
            string path = RevisionPath();
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            string temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                using (var stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    JsonSerializer.Serialize(stream, revision);
                    stream.Flush(true);
                }
                File.Move(temporary, path, true);
            }
            finally { if (File.Exists(temporary)) File.Delete(temporary); }
        }
    }
}
