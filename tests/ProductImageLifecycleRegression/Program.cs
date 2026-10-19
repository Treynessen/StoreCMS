using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Security.Cryptography;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Treynessen.Database.Context;
using Treynessen.ImagesManagement;
using Treynessen.Functions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Formats.Jpeg;

class Program
{
    static int checks;
    static readonly string Root = Path.Combine(Path.GetTempPath(), "StoreCMS-images-" + Guid.NewGuid().ToString("N"));
    static void Check(bool ok, string message) { if (!ok) throw new Exception(message); Interlocked.Increment(ref checks); Console.WriteLine("PASS " + message); }
    class Env : IHostingEnvironment
    {
        public string EnvironmentName { get; set; }
        public string ApplicationName { get; set; }
        public string WebRootPath { get; set; }
        public IFileProvider WebRootFileProvider { get; set; }
        public string ContentRootPath { get; set; }
        public IFileProvider ContentRootFileProvider { get; set; }
    }
    class Fixture
    {
        public Env env;
        public CMSDatabase db = new CMSDatabase();
        public DefaultHttpContext ctx;
        public string folder;
        public Treynessen.Database.Entities.ProductPage product;
        public IDataProtectionProvider provider = new EphemeralDataProtectionProvider();
        public Fixture(string root = null, int id = 41)
        {
            string path = (root ?? Path.Combine(Root, Guid.NewGuid().ToString("N"))).Replace('\\', '/') + "/";
            env = new Env { ContentRootPath = path, WebRootPath = path + "Storage/" };
            product = new Treynessen.Database.Entities.ProductPage { ID = id, Alias = "image", PageName = "Тест" };
            db.ProductPages.Add(product);
            folder = env.WebRootPath + "images/products/" + id + "/";
            Directory.CreateDirectory(folder);
            ctx = new DefaultHttpContext { RequestServices = new ServiceCollection().AddSingleton<IHostingEnvironment>(env).AddSingleton(provider).BuildServiceProvider() };
        }
        public string Name(int n) => "image" + (n == 1 ? "" : "_" + n) + ".jpg";
        public void Put(int n)
        {
            using (var image = new Image<Rgba32>(300 + n, 200 + n)) image.Save(folder + Name(n));
            string shortPath = "/images/products/" + product.ID + "/" + Name(n);
            db.Images.Add(new Treynessen.Database.Entities.Image { ShortPath = shortPath, ShortPathHash = OtherFunctions.GetHashFromString(shortPath), FullName = Name(n), Width = (uint)(300+n), Height = (uint)(200+n) });
            File.WriteAllText(folder + Path.GetFileNameWithoutExtension(Name(n)) + "_150x100_q50.jpg", "derivative-" + n);
        }
        public string Token(string name) => ProductImageOperations.CreateToken(product, folder, name, provider);
        public int Delete(string name, string token) => ImagesManagementFunctions.DeleteProductImage(db, product.ID, name, token, ctx);
        public void Upload(int width, Action onOpen = null)
        {
            using (var stream = new MemoryStream())
            {
                using (var image = new Image<Rgba32>(width, width + 7)) image.Save(stream, new JpegEncoder());
                stream.Position = 0;
                var file = new CallbackFile(stream, onOpen);
                ImagesManagementFunctions.UploadProductImageToServer(db, file, product.ID, ctx, out bool ok);
                Check(ok, "загрузка " + width);
            }
        }
    }
    class CallbackFile : IFormFile
    {
        readonly FormFile inner;
        readonly Action open;
        public CallbackFile(Stream stream, Action open) { inner = new FormFile(stream, 0, stream.Length, "uploadedFile", "test.jpg"); this.open = open; }
        public string ContentType => inner.ContentType;
        public string ContentDisposition => inner.ContentDisposition;
        public IHeaderDictionary Headers => inner.Headers;
        public long Length => inner.Length;
        public string Name => inner.Name;
        public string FileName => inner.FileName;
        public Stream OpenReadStream() { open?.Invoke(); return inner.OpenReadStream(); }
        public void CopyTo(Stream target) => inner.CopyTo(target);
        public Task CopyToAsync(Stream target, CancellationToken cancellationToken = default(CancellationToken)) => inner.CopyToAsync(target, cancellationToken);
    }
    static string Hash(string path) { using (var sha = SHA256.Create()) return Convert.ToBase64String(sha.ComputeHash(File.ReadAllBytes(path))); }
    static void WriteAcl(string folder, bool deny)
    {
        string code = "$ErrorActionPreference='Stop'; $acl=[IO.Directory]::GetAccessControl($env:STORECMS_TEST_STAGING); "
            + "$rule=[Security.AccessControl.FileSystemAccessRule]::new([Security.Principal.WindowsIdentity]::GetCurrent().User,'CreateFiles','Deny'); "
            + (deny ? "$acl.AddAccessRule($rule); " : "$acl.RemoveAccessRuleSpecific($rule); ")
            + "[IO.Directory]::SetAccessControl($env:STORECMS_TEST_STAGING,$acl)";
        var start = new ProcessStartInfo("powershell.exe", "-NoProfile -NonInteractive -Command \"" + code + "\"")
            { UseShellExecute=false, CreateNoWindow=true, RedirectStandardOutput=true, RedirectStandardError=true };
        start.EnvironmentVariables["STORECMS_TEST_STAGING"] = folder;
        using(var process=Process.Start(start))
            if(!process.WaitForExit(10000) || process.ExitCode!=0) throw new Exception("ACL: " + process.StandardError.ReadToEnd());
    }
    static void Deletions()
    {
        foreach (int[] numbers in new[] { new[] { 1 }, new[] { 1,2,3 }, new[] { 1,3,4 }, new[] { 1,3,5 } })
        foreach (int selected in numbers)
        {
            var f = new Fixture(); foreach (int n in numbers) f.Put(n);
            var hashes = numbers.ToDictionary(n => n, n => Hash(f.folder + f.Name(n)));
            string name = f.Name(selected), token = f.Token(name);
            Check(f.Delete(name, token) == 200, "удаление строки " + selected + " из " + string.Join(",", numbers));
            int[] survivors = numbers.Where(n => n != selected).ToArray();
            Check(ProductImageOperations.GetOriginalNames(f.folder, "image").SequenceEqual(Enumerable.Range(1, survivors.Length).Select(f.Name)), "плотная нумерация");
            for (int i = 0; i < survivors.Length; i++)
            {
                Check(Hash(f.folder + f.Name(i+1)) == hashes[survivors[i]], "SHA-256 оставшегося оригинала");
                Check(File.ReadAllText(f.folder + Path.GetFileNameWithoutExtension(f.Name(i+1)) + "_150x100_q50.jpg") == "derivative-" + survivors[i], "перенос производного");
                Check(f.db.Images.Single(x => x.FullName == f.Name(i+1)).Width == 300 + survivors[i], "размеры после переноса, двойник БД");
            }
            var before = Directory.GetFiles(f.folder).ToDictionary(p => p, Hash);
            Check(f.Delete(name, token) == 409, "повтор старого DELETE: 409");
            Check(before.All(p => File.Exists(p.Key) && Hash(p.Key) == p.Value), "повтор ничего не удалил");
        }
        {
            var f = new Fixture(); f.Put(1); f.Put(2); f.Put(3);
            string first = f.Token(f.Name(1)), second = f.Token(f.Name(2));
            Check(f.Delete(f.Name(1), first) == 200 && f.Delete(f.Name(2), second) == 409, "две вкладки, старое имя принадлежит другому оригиналу");
            string token = f.Token(f.Name(1));
            foreach (string name in new[] { "../image.jpg", "image_2.jpg", "baimage.jpg", "/image.jpg", "..\\image.jpg" })
                Check(f.Delete(name, token) == 409, "подмена имени/семейства: " + name);
            var other = new Fixture(); other.Put(1);
            Check(ImagesManagementFunctions.DeleteProductImage(other.db, 41, "image.jpg", token, other.ctx) == 409, "токен другой установки/товара отклонён");
            var p = new Treynessen.Database.Entities.ProductPage { ID = 42, Alias = "image" };
            Check(!ProductImageOperations.MatchesToken(p, f.folder, "image.jpg", token, f.provider), "токен связан с ID товара");
            Check(f.Delete("image.jpg", null) == 409 && f.Delete("image.jpg", token + "x") == 409, "нет/испорчен токен");
            f.Upload(901);
            string added = ProductImageOperations.GetOriginalNames(f.folder, "image").Last();
            Check(f.Delete(added, f.Token(added)) == 200 && f.Delete("image.jpg", token) == 409, "возврат к прежнему набору не оживляет старый токен");
            File.WriteAllText(f.folder + "image.jpg", "replacement");
            Check(f.Delete("image.jpg", token) == 409, "замена байтов при прежнем имени");
        }
    }
    static void Uploads()
    {
        var f = new Fixture(); var g = new Fixture(f.env.ContentRootPath.TrimEnd('/'));
        using (var barrier = new Barrier(2))
        {
            Action open = () => { if (!barrier.SignalAndWait(10000)) throw new Exception("barrier timeout"); };
            var a = Task.Run(() => f.Upload(333, open));
            var b = Task.Run(() => g.Upload(555, open));
            Check(Task.WaitAll(new[] { a,b }, 20000), "параллельные upload завершились, отдельные контексты");
        }
        Check(ProductImageOperations.GetOriginalNames(f.folder, "image").Length == 2, "одно свободное имя, два оригинала");
        foreach (var db in new[] { f.db, g.db })
        {
            var metadata = db.Images.Single();
            using (var image = Image.Load(f.folder + metadata.FullName))
                Check(image.Width == metadata.Width && image.Height == metadata.Height, "метаданные соответствуют опубликованному файлу");
        }
        Check(Hash(f.folder + "image.jpg") != Hash(f.folder + "image_2.jpg"), "содержимое загрузок различается");
        Check(!Directory.GetFiles(Path.Combine(f.env.ContentRootPath, "App_Data/image-staging")).Any(), "временные файлы очищены");
        {
            var a = new Fixture(); var b = new Fixture(a.env.ContentRootPath.TrimEnd('/'));
            using (var saved = new ManualResetEventSlim())
            using (var release = new ManualResetEventSlim())
            {
                a.db.OnSave = () => { Check(File.Exists(a.folder + "image.jpg"), "оригинал готов до SaveChanges"); saved.Set(); if (!release.Wait(10000)) throw new Exception("release timeout"); };
                var first = Task.Run(() => a.Upload(401));
                Check(saved.Wait(10000), "первый запрос удерживает критическую секцию");
                var second = Task.Run(() => b.Upload(402));
                Check(!second.Wait(150), "второй запрос ждёт публикацию/метаданные первого");
                release.Set(); Check(Task.WaitAll(new[] { first,second }, 10000), "контролируемое чередование завершено");
            }
        }
        {
            var a = new Fixture(); var b = new Fixture(a.env.ContentRootPath.TrimEnd('/'), 42);
            using (ProductImageOperations.Acquire(a.folder))
            {
                var task = Task.Run(() => b.Upload(501));
                Check(task.Wait(10000), "другой товар не блокируется");
            }
        }
        {
            var a = new Fixture(); a.db.ThrowSave = 1; bool failed = false;
            try { a.Upload(600); } catch (Microsoft.EntityFrameworkCore.DbUpdateException) { failed = true; }
            Check(failed && File.Exists(a.folder + "image.jpg"), "ошибка метаданных: нет ложного успеха, опубликованный оригинал сохранён");
            string preserved = Hash(a.folder + "image.jpg");
            new Fixture(a.env.ContentRootPath.TrimEnd('/')).Upload(601);
            Check(Hash(a.folder + "image.jpg") == preserved && File.Exists(a.folder + "image_2.jpg"), "отдельная явная загрузка после сбоя не перезаписывает оставшийся оригинал");
        }
        {
            var a = new Fixture(); Directory.CreateDirectory(a.folder + "image.jpg"); bool failed = false;
            try { a.Upload(700); } catch (IOException) { failed = true; } catch (UnauthorizedAccessException) { failed = true; }
            Check(failed && a.db.Saves == 0, "отказ публикации: БД не записана");
        }
        {
            var a = new Fixture(); Directory.CreateDirectory(Path.Combine(a.env.ContentRootPath, "App_Data"));
            File.WriteAllText(Path.Combine(a.env.ContentRootPath, "App_Data/image-staging"), "blocked"); bool failed = false;
            try { a.Upload(701); } catch (IOException) { failed = true; }
            Check(failed && a.db.Saves == 0 && !Directory.GetFiles(a.folder).Any(), "отказ временного хранилища: нет мутации/успеха");
        }
        {
            var a = new Fixture();
            var staging = Directory.CreateDirectory(Path.Combine(a.env.ContentRootPath, "App_Data/image-staging"));
            bool failed = false;
            try
            {
                WriteAcl(staging.FullName, true);
                try { a.Upload(702); } catch (UnauthorizedAccessException) { failed = true; }
            }
            finally { WriteAcl(staging.FullName, false); }
            Check(failed && a.db.Saves == 0 && !Directory.GetFiles(a.folder).Any(), "отказ открытия временного файла для записи по ACL");
        }
    }
    static void Generation()
    {
        var f = new Fixture(); f.Put(1);
        File.Delete(f.folder + "image_150x100_q50.jpg");
        new ImageHandler("/images/products/41/image.jpg", false, f.db, f.env).ImageResizing(maxWidth:150,maxHeight:150).ImageComprassion(50).ApplySettings();
        Check(File.Exists(f.folder + "image_150x100_q50.jpg"), "сохранены правила генерации");
        File.Delete(f.folder + "image.jpg");
        new ImageHandler("/images/products/41/image.jpg", false, f.db, f.env);
        Check(f.db.Images.Count == 0 && !File.Exists(f.folder + "image_150x100_q50.jpg"), "сохранена автоматическая очистка");
    }
    static void Processes()
    {
        string root = Path.Combine(Root, "multiprocess");
        string dll = typeof(Program).Assembly.Location;
        var a = Process.Start(new ProcessStartInfo("dotnet", "\"" + dll + "\" --child \"" + root + "\" 811") { UseShellExecute = false, CreateNoWindow = true });
        var b = Process.Start(new ProcessStartInfo("dotnet", "\"" + dll + "\" --child \"" + root + "\" 822") { UseShellExecute = false, CreateNoWindow = true });
        Check(a.WaitForExit(20000) && b.WaitForExit(20000) && a.ExitCode == 0 && b.ExitCode == 0, "два процесса upload, один сеанс ОС");
        var f = new Fixture(root);
        var widths = ProductImageOperations.GetOriginalNames(f.folder, "image").Select(n => { using (var im=Image.Load(f.folder+n)) return im.Width; }).OrderBy(n=>n);
        Check(widths.SequenceEqual(new[] {811,822}), "межпроцессная сохранность обоих оригиналов");
    }
    static int Main(string[] args)
    {
        try
        {
            if (args.Length > 0 && args[0] == "--child")
            {
                var f = new Fixture(args[1]);
                f.Upload(int.Parse(args[2]), () => {
                    File.WriteAllText(Path.Combine(args[1], args[2]+".ready"), "ready");
                    var watch=Stopwatch.StartNew();
                    while (Directory.GetFiles(args[1], "*.ready").Length < 2) { if(watch.ElapsedMilliseconds>10000) throw new Exception("child timeout"); Thread.Sleep(10); }
                }); return 0;
            }
            Deletions(); Uploads(); Generation(); Processes();
            Console.WriteLine("TOTAL PASS " + checks + "; БД/SQL заменены двойником; crash/распределённая ФС не проверены."); return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine(ex); return 1; }
    }
}
