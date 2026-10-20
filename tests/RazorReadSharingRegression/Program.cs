using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Extensions.FileProviders;
using Microsoft.Win32.SafeHandles;
using Treynessen.TemplatesManagement;

internal static class Program
{
    [DllImport("kernel32.dll", EntryPoint = "CreateFileW", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern SafeFileHandle CreateFile(string path, uint access, uint share, IntPtr security, uint disposition, uint flags, IntPtr template);
    private static int checks;
    private static void Check(bool ok, string name)
    {
        if (!ok) throw new Exception(name);
        checks++;
        Console.WriteLine("PASS " + name);
    }
    private static string Read(IFileInfo file)
    {
        using (var stream = file.CreateReadStream())
        using (var reader = new StreamReader(stream)) return reader.ReadToEnd();
    }
    private static void Main()
    {
        string root = Path.Combine(Path.GetTempPath(), "StoreCMS-E04-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            using (var physical = new PhysicalFileProvider(root))
            {
                var wrapper = new GeneratedRazorFileProvider(physical);
                string[] paths = { "Views/Templates/a.cshtml", "Views/Chunks/a.cshtml", "Views/Templates/sub/a.cshtml",
                    "Views/Templates/_ViewImports.cshtml", "Views/Chunks/_ViewStart.cshtml", "Views/AdminPanel/a.cshtml",
                    "Settings/product_block.cshtml", "Views/Templates/a.txt", "Views/TemplatesSibling/a.cshtml" };
                for (int i = 0; i < paths.Length; i++)
                {
                    string target = Path.Combine(root, paths[i]);
                    Directory.CreateDirectory(Path.GetDirectoryName(target));
                    File.WriteAllText(target, "OLD");
                    var original = physical.GetFileInfo(paths[i]);
                    var wrapped = wrapper.GetFileInfo(paths[i]);
                    Check((original.GetType() != wrapped.GetType()) == (i < 3), "scope " + paths[i]);
                    Check(wrapped.Exists == original.Exists && wrapped.Length == original.Length &&
                        wrapped.PhysicalPath == original.PhysicalPath && wrapped.Name == original.Name &&
                        wrapped.LastModified == original.LastModified && wrapped.IsDirectory == original.IsDirectory,
                        "metadata " + paths[i]);
                    using (var held = CreateFile(target, 0x10000, 7, IntPtr.Zero, 3, 0x80, IntPtr.Zero))
                    {
                        Check(!held.IsInvalid, "DELETE handle " + paths[i]);
                        bool sharing = false;
                        try { Read(original); }
                        catch (IOException e) { sharing = (e.HResult & 0xffff) == 32; }
                        Check(sharing, "control sharing violation " + paths[i]);
                        if (i < 3) Check(Read(wrapped) == "OLD", "shared read " + paths[i]);
                        else
                        {
                            sharing = false;
                            try { Read(wrapped); }
                            catch (IOException e) { sharing = (e.HResult & 0xffff) == 32; }
                            Check(sharing, "standard read preserved " + paths[i]);
                        }
                    }
                }
                Check(wrapper.GetFileInfo("missing.cshtml").GetType() == physical.GetFileInfo("missing.cshtml").GetType(), "missing preserved");
                Check(wrapper.GetFileInfo("Views/Templates").IsDirectory == physical.GetFileInfo("Views/Templates").IsDirectory &&
                    wrapper.GetFileInfo("Views/Templates").GetType() == physical.GetFileInfo("Views/Templates").GetType(), "directory preserved");
                Check(!wrapper.GetFileInfo("../outside.cshtml").Exists, "traversal preserved");
                Check(wrapper.GetDirectoryContents("Views/Templates").Select(f => f.Name).OrderBy(n => n)
                    .SequenceEqual(physical.GetDirectoryContents("Views/Templates").Select(f => f.Name).OrderBy(n => n)), "directory listing preserved");
                var custom = new NullFileProvider();
                var providers = new List<IFileProvider> { custom, physical };
                GeneratedRazorFileProvider.WrapPhysicalProviders(providers);
                var first = providers[1];
                GeneratedRazorFileProvider.WrapPhysicalProviders(providers);
                Check(ReferenceEquals(providers[0], custom) && ReferenceEquals(first, providers[1]), "order custom provider and idempotency");
                foreach (string path in new[] { paths[0], paths[1], paths[3] })
                {
                    var token = physical.Watch(path);
                    Check(ReferenceEquals(token, wrapper.Watch(path)), "same token " + path);
                    using (var changed = new ManualResetEventSlim())
                    using (token.RegisterChangeCallback(_ => changed.Set(), null))
                    {
                        File.WriteAllText(Path.Combine(root, path), "NEW");
                        Check(changed.Wait(10000) && token.HasChanged, "watch callback " + path);
                    }
                }
                var file = wrapper.GetFileInfo(paths[0]);
                using (var stream = file.CreateReadStream())
                {
                    string old = Path.Combine(root, paths[0]);
                    string moved = old + ".old";
                    File.Move(old, moved);
                    File.WriteAllText(old, "NEXT");
                    using (var reader = new StreamReader(stream)) Check(reader.ReadToEnd() == "NEW", "open reader keeps whole old version");
                    Check(Read(wrapper.GetFileInfo(paths[0])) == "NEXT", "new reader sees new version");
                }
            }
            Console.WriteLine("TOTAL PASS " + checks);
        }
        finally { Directory.Delete(root, true); }
    }
}
