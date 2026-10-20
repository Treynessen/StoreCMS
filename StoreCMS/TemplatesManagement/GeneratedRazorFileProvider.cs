using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace Treynessen.TemplatesManagement
{
    public sealed class GeneratedRazorFileProvider : IFileProvider
    {
        private readonly PhysicalFileProvider physical;
        private readonly string templates;
        private readonly string chunks;
        private readonly StringComparison comparison = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

        public GeneratedRazorFileProvider(PhysicalFileProvider physical)
        {
            this.physical = physical ?? throw new ArgumentNullException(nameof(physical));
            templates = Path.Combine(physical.Root, "Views", "Templates") + Path.DirectorySeparatorChar;
            chunks = Path.Combine(physical.Root, "Views", "Chunks") + Path.DirectorySeparatorChar;
        }

        public static void WrapPhysicalProviders(IList<IFileProvider> providers)
        {
            for (int i = 0; i < providers.Count; i++)
                if (providers[i] is PhysicalFileProvider physical)
                    providers[i] = new GeneratedRazorFileProvider(physical);
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            IFileInfo file = physical.GetFileInfo(subpath);
            string path = file.PhysicalPath;
            if (!file.Exists || file.IsDirectory || path == null ||
                !path.EndsWith(".cshtml", comparison) ||
                (!path.StartsWith(templates, comparison) && !path.StartsWith(chunks, comparison)) ||
                file.Name.Equals("_ViewImports.cshtml", comparison) ||
                file.Name.Equals("_ViewStart.cshtml", comparison))
                return file;
            return new GeneratedRazorFileInfo(file);
        }

        public IDirectoryContents GetDirectoryContents(string subpath) => physical.GetDirectoryContents(subpath);
        public IChangeToken Watch(string filter) => physical.Watch(filter);

        private sealed class GeneratedRazorFileInfo : IFileInfo
        {
            private readonly IFileInfo file;
            public GeneratedRazorFileInfo(IFileInfo file) { this.file = file; }
            public bool Exists => file.Exists;
            public long Length => file.Length;
            public string PhysicalPath => file.PhysicalPath;
            public string Name => file.Name;
            public DateTimeOffset LastModified => file.LastModified;
            public bool IsDirectory => file.IsDirectory;
            public Stream CreateReadStream() => new FileStream(PhysicalPath, FileMode.Open, FileAccess.Read,
                FileShare.ReadWrite | FileShare.Delete, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan);
        }
    }
}
