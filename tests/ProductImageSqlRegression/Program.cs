using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using MySql.Data.MySqlClient;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Formats.Jpeg;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;
using Treynessen.ImagesManagement;

class SqlRegression
{
    static int checks;
    static MySqlConnection connection;
    static DbContextOptions<CMSDatabase> options;
    static IHostingEnvironment env;
    static DefaultHttpContext ctx;
    static IDataProtectionProvider provider = new EphemeralDataProtectionProvider();
    static string folder;
    static void Check(bool ok, string label) { if (!ok) throw new InvalidOperationException("CHECK: " + label); checks++; Console.WriteLine("PASS " + label); }
    static void Sql(string sql) { using (var cmd = new MySqlCommand(sql, connection)) cmd.ExecuteNonQuery(); }
    static string Scalar(string sql) { using (var cmd = new MySqlCommand(sql, connection)) return Convert.ToString(cmd.ExecuteScalar()); }
    static void TemporaryEntity(Type type, string table)
    {
        var columns = type.GetProperties().Where(p => p.PropertyType.IsValueType || p.PropertyType == typeof(string)).Select(p => {
            Type scalar = Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType;
            string sqlType = p.Name == "ShortPath" ? "varchar(255)" : scalar == typeof(string) ? "longtext" : scalar == typeof(DateTime) ? "datetime(6)" : scalar == typeof(bool) ? "tinyint(1)" : scalar == typeof(uint) ? "int unsigned" : "int";
            return "`" + p.Name + "` " + sqlType + (p.Name == "ID" ? " NOT NULL AUTO_INCREMENT PRIMARY KEY" : " NULL") + (p.Name == "ShortPath" ? " UNIQUE" : "");
        });
        Sql("CREATE TEMPORARY TABLE `" + table + "`(" + string.Join(",", columns) + ") ENGINE=InnoDB");
    }
    class Env : IHostingEnvironment
    {
        public string EnvironmentName {get;set;}
        public string ApplicationName {get;set;}
        public string WebRootPath {get;set;}
        public IFileProvider WebRootFileProvider {get;set;}
        public string ContentRootPath {get;set;}
        public IFileProvider ContentRootFileProvider {get;set;}
    }
    class FailingContext : CMSDatabase
    {
        public FailingContext() : base(options) { }
        public override int SaveChanges() { throw new DbUpdateException("Тестовый отказ сохранения.", new IOException()); }
    }
    static string Name(int n) => "image" + (n == 1 ? "" : "_" + n) + ".jpg";
    static string Hash(string p) { using(var sha=SHA256.Create()) return Convert.ToBase64String(sha.ComputeHash(File.ReadAllBytes(p))); }
    static ProductPage Product() => new ProductPage { ID=41, Alias="image", PageName="Тест", Title="Тест", RequestPath="/test", PreviousPageID=1, LastUpdate=DateTime.Now };
    static void Reset()
    {
        // [Комментарий ИИ-агента]: Обе таблицы уже созданы как TEMPORARY в единственном открытом соединении; постоянные таблицы скрыты.
        Sql("DELETE FROM Images"); Sql("DELETE FROM ProductPages");
        foreach (var p in Directory.GetFiles(folder)) File.Delete(p);
        using(var db=new CMSDatabase(options)) { db.ProductPages.Add(Product()); db.SaveChanges(); }
    }
    static void Upload(CMSDatabase db, int w)
    {
        UploadAttempt(db, w, out bool ok);
        Check(ok,"production upload и SQL " + w);
    }
    static void UploadAttempt(CMSDatabase db, int w, out bool ok)
    {
        using(var stream=new MemoryStream())
        {
            using(var im=new Image<Rgba32>(w,w+9)) im.Save(stream,new JpegEncoder());
            stream.Position=0;
            ImagesManagementFunctions.UploadProductImageToServer(db,new FormFile(stream,0,stream.Length,"uploadedFile","test.jpg"),41,ctx,out ok);
        }
    }
    static void Run()
    {
        foreach(int[] nums in new[]{new[]{1},new[]{1,2,3},new[]{1,3,4},new[]{1,3,5}})
        foreach(int selected in nums)
        {
            Reset();
            using(var db=new CMSDatabase(options))
            {
                foreach(int n in nums)
                {
                    using(var im=new Image<Rgba32>(300+n,200+n)) im.Save(folder+Name(n));
                    string path="/images/products/41/"+Name(n);
                    db.Images.Add(new Treynessen.Database.Entities.Image {ShortPath=path,ShortPathHash=Treynessen.Functions.OtherFunctions.GetHashFromString(path),FullName=Name(n),Width=(uint)(300+n),Height=(uint)(200+n)});
                    File.WriteAllText(folder+Path.GetFileNameWithoutExtension(Name(n))+"_100x100_q50.jpg","variant-"+n);
                }
                db.SaveChanges();
                var hashes=nums.ToDictionary(n=>n,n=>Hash(folder+Name(n)));
                string token=ProductImageOperations.CreateToken(Product(),folder,Name(selected),provider);
                Check(ImagesManagementFunctions.DeleteProductImage(db,41,Name(selected),token,ctx)==200,"production delete, SQL/tracking " + string.Join(",",nums) + " / " + selected);
                Check(ImagesManagementFunctions.DeleteProductImage(db,41,Name(selected),token,ctx)==409,"повтор SQL DELETE даёт 409");
                int[] survivors=nums.Where(n=>n!=selected).ToArray();
                using(var verify=new CMSDatabase(options))
                {
                    var metadata=verify.Images.AsNoTracking().OrderBy(i=>i.FullName).ToArray();
                    Check(metadata.Length==survivors.Length,"число строк в настоящей TEMPORARY Images");
                    for(int i=0;i<survivors.Length;i++)
                    {
                        Check(Hash(folder+Name(i+1))==hashes[survivors[i]],"SQL: SHA-256 оставшегося оригинала");
                        var row=metadata.Single(m=>m.FullName==Name(i+1));
                        Check(row.Width==300+survivors[i] && row.ShortPath=="/images/products/41/"+Name(i+1),"SQL: путь/размеры после последовательного EF rename");
                        Check(File.ReadAllText(folder+Path.GetFileNameWithoutExtension(Name(i+1))+"_100x100_q50.jpg")=="variant-"+survivors[i],"SQL: производные сохранены");
                    }
                }
            }
        }
        Reset();
        using(var db=new CMSDatabase(options)) { Upload(db,444); Upload(db,666); }
        using(var db=new CMSDatabase(options)) foreach(var row in db.Images.AsNoTracking().ToArray())
            using(var im=SixLabors.ImageSharp.Image.Load(folder+row.FullName)) Check(im.Width==row.Width && im.Height==row.Height,"SQL upload: реальные размеры/готовый оригинал");
        Reset(); bool failed=false, ok=true;
        using(var db=new FailingContext()) try { UploadAttempt(db,777,out ok); } catch(DbUpdateException) { failed=true; }
        Check(failed && !ok && Scalar("SELECT COUNT(*) FROM Images")=="0" && File.Exists(folder+"image.jpg"),"отказ до SQL: успех false, строки нет, допустимый файловый сирота сохранён");
        string preserved=Hash(folder+"image.jpg");
        using(var db=new CMSDatabase(options)) Upload(db,778);
        Check(Hash(folder+"image.jpg")==preserved && File.Exists(folder+"image_2.jpg"),"явная новая загрузка не перезаписывает сироту");
    }
    class CommitFault : IObserver<DiagnosticListener>, IObserver<KeyValuePair<string,object>>, IDisposable
    {
        readonly List<IDisposable> subscriptions = new List<IDisposable>();
        readonly IDisposable all;
        public bool Armed, Fired;
        public string PublishedHash;
        public CommitFault() { all=DiagnosticListener.AllListeners.Subscribe(this); }
        public void OnNext(DiagnosticListener value)
        {
            if(value.Name=="Microsoft.EntityFrameworkCore") subscriptions.Add(value.Subscribe(this));
        }
        public void OnNext(KeyValuePair<string,object> value)
        {
            if(Armed && value.Key=="Microsoft.EntityFrameworkCore.Database.Transaction.TransactionCommitted")
            {
                Armed=false; Fired=true;
                PublishedHash=Hash(folder+"image.jpg");
                throw new IOException("Тестовый отказ после фактического commit MySQL.");
            }
        }
        public void OnCompleted() { }
        public void OnError(Exception error) { }
        public void Dispose() { foreach(var subscription in subscriptions) subscription.Dispose(); all.Dispose(); }
    }
    static void CommitOutcomeRegression()
    {
        Reset();
        using(var fault=new CommitFault())
        {
            bool ok=true, failed=false;
            using(var db=new CMSDatabase(options))
            {
                fault.Armed=true;
                try { UploadAttempt(db,831,out ok); } catch(Exception) { failed=true; }
            }
            Check(fault.Fired && failed && !ok,"IR-01: исключение после commit, успех false");
            Check(Scalar("SELECT COUNT(*) FROM Images")=="1","IR-01: прямой SQL после Dispose контекста подтверждает сохранённую строку");
            Check(Scalar("SELECT CONCAT(FullName,':',Width,':',Height) FROM Images")=="image.jpg:831:840","IR-01: сохранены правильные метаданные");
            Check(File.Exists(folder+"image.jpg"),"IR-01: зарегистрированный оригинал существует");
            Check(Hash(folder+"image.jpg")==fault.PublishedHash,"IR-01: SHA-256 оригинала не изменён после исключения");
            Check(Directory.GetFiles(folder).Length==1 && !Directory.GetFiles(Path.Combine(env.ContentRootPath,"App_Data/image-staging")).Any(),"IR-01: временные файлы очищены, автоматического повтора нет");
        }
    }
    static int Main(string[] args)
    {
        try
        {
            string agents=File.ReadAllText(Path.Combine(args[0],"AGENTS.md"));
            string secret=Regex.Match(agents,@"\| Пароль \| `([^`]+)`").Groups[1].Value;
            if(string.IsNullOrEmpty(secret)) throw new InvalidOperationException("Нет тестовых реквизитов.");
            using(connection=new MySqlConnection(new MySqlConnectionStringBuilder {Server="127.0.0.1",Port=3306,Database="test",UserID="root",Password=secret,Pooling=false,SslMode=MySqlSslMode.Required,ConnectionTimeout=5}.ConnectionString))
            {
                connection.Open(); Check(Scalar("SELECT DATABASE()") == "test", "read-only подключение к существующей test");
                string before=Scalar("SELECT GROUP_CONCAT(TABLE_NAME ORDER BY TABLE_NAME) FROM information_schema.tables WHERE TABLE_SCHEMA=DATABASE()");
                string counts=Scalar("SELECT CONCAT((SELECT COUNT(*) FROM Images),':',(SELECT COUNT(*) FROM ProductPages))");
                TemporaryEntity(typeof(Treynessen.Database.Entities.Image), "Images");
                TemporaryEntity(typeof(ProductPage), "ProductPages");
                try
                {
                    options=new DbContextOptionsBuilder<CMSDatabase>().UseMySql(connection).Options;
                    string root=Path.Combine(Path.GetTempPath(),"StoreCMS-image-sql-"+Guid.NewGuid().ToString("N")).Replace('\\','/');
                    env=new Env{ContentRootPath=root,WebRootPath=root+"/Storage/"};
                    folder=root+"/Storage/images/products/41/";Directory.CreateDirectory(folder);
                    ctx=new DefaultHttpContext{RequestServices=new ServiceCollection().AddSingleton(env).AddSingleton(provider).BuildServiceProvider()};
                    CommitOutcomeRegression();
                    Run();
                }
                finally { Sql("DROP TEMPORARY TABLE IF EXISTS Images,ProductPages"); }
                Check(before==Scalar("SELECT GROUP_CONCAT(TABLE_NAME ORDER BY TABLE_NAME) FROM information_schema.tables WHERE TABLE_SCHEMA=DATABASE()"),"состав постоянных таблиц сохранён");
                Check(counts==Scalar("SELECT CONCAT((SELECT COUNT(*) FROM Images),':',(SELECT COUNT(*) FROM ProductPages))"),"число постоянных строк Images/ProductPages сохранено");
            }
            Console.WriteLine("TOTAL SQL PASS "+checks+"; только TEMPORARY-таблицы; конкурентный SQL не заявляется.");return 0;
        }
        catch(Exception ex) { Console.Error.WriteLine("FAIL "+ex.GetType().Name); if(ex is MySqlException mysql) Console.Error.WriteLine("MySQL "+mysql.Number); if(ex.Message.StartsWith("CHECK:")) Console.Error.WriteLine(ex.Message); return 1; }
    }
}
