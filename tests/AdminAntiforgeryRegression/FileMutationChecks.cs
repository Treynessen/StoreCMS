using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;
using Treynessen.Security;
using System.Text.RegularExpressions;
using System.Net;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

internal static partial class AntiforgeryRegression
{
    static async Task FileStatus(HttpClient client, string method, string path, string page, int expected, params string[] fields)
    {
        using (var response = await Send(client,method,path,page,fields))
        {
            Check((int)response.StatusCode == expected, "file mutation " + page + " status " + (int)response.StatusCode);
            Check(response.Headers.CacheControl.NoStore, "file mutation no-store");
            Check((response.Headers.Location != null) == (page == "39" || page == "40" || page == "41"), "preserved file Location header contract");
        }
    }
    static void TemporaryEntity(Type type, string table)
    {
        var columns = type.GetProperties().Where(p => p.PropertyType.IsValueType || p.PropertyType == typeof(string)).Select(p => {
            Type scalar = Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType;
            string sqlType = scalar == typeof(string) ? "longtext" : scalar == typeof(DateTime) ? "datetime(6)" : scalar == typeof(bool) ? "tinyint(1)" : scalar == typeof(uint) ? "int unsigned" : "int";
            return "`" + p.Name + "` " + sqlType + (p.Name == "ID" ? " NOT NULL AUTO_INCREMENT PRIMARY KEY" : " NULL");
        });
        // [Комментарий ИИ-агента]: Создаются только TEMPORARY-таблицы скалярной формы двух сущностей, без DDL постоянных таблиц и без EnsureCreated.
        Sql("CREATE TEMPORARY TABLE `" + table + "`(" + string.Join(",", columns) + ") ENGINE=InnoDB");
    }
    static async Task FileMutationFlows(HttpClient client, string token)
    {
        string contentRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../StoreCMS"));
        Check(!File.Exists(Path.Combine(contentRoot, "Settings/config.json")), "file checks require isolated copy without config.json");
        string name = "csrf233_" + Guid.NewGuid().ToString("N");
        string storage = Path.Combine(contentRoot, "Storage");
        string folder = Path.GetFullPath(Path.Combine(storage, name));
        const int productId = 2000000233;
        string productFolder = Path.GetFullPath(Path.Combine(storage, "images/products/" + productId));
        Check(!Directory.Exists(folder) && !Directory.Exists(productFolder), "owned filesystem fixtures absent before setup");
        Directory.CreateDirectory(folder);
        TemporaryEntity(typeof(ProductPage), "ProductPages");
        TemporaryEntity(typeof(Treynessen.Database.Entities.Image), "Images");
        try
        {
            using (var db = new CMSDatabase(options))
            {
                db.ProductPages.Add(new ProductPage {ID=productId, Alias="csrf-fixture", PageName="fixture", Title="fixture", RequestPath="/csrf-fixture", LastUpdate=DateTime.UtcNow});
                db.SaveChanges();
            }
            byte[] png;
            using (var stream = new MemoryStream())
            {
                using (var image = new Image<Rgba32>(2,2)) image.Save(stream, new SixLabors.ImageSharp.Formats.Png.PngEncoder());
                png = stream.ToArray();
            }
            foreach (string page in new[]{"24","35"})
            {
                foreach (string csrf in new[]{(string)null,"broken",token})
                {
                    int imagesBefore = int.Parse(Scalar("SELECT COUNT(*) FROM Images"));
                    using (var request = new HttpRequestMessage(HttpMethod.Put, "/admin"))
                    using (var form = new MultipartFormDataContent())
                    {
                        form.Add(new StringContent(page),"pageID");
                        form.Add(new StringContent(productId.ToString()),"itemID");
                        form.Add(new StringContent(name),"path");
                        form.Add(new ByteArrayContent(png),"uploadedFile","fixture.png");
                        if (csrf != null) request.Headers.Add(AdminAntiforgery.HeaderName,csrf);
                        request.Content = form;
                        requests++;
                        using (var response = await client.SendAsync(request))
                            Check((int)response.StatusCode == (csrf == token ? 200 : 400), "real PNG upload with valid/missing/corrupt CSRF");
                    }
                    bool created = page == "24" ? File.Exists(Path.Combine(productFolder,"csrf-fixture.jpg")) : File.Exists(Path.Combine(folder,"fixture.png"));
                    Check(created == (csrf == token), "file exists only after valid CSRF");
                    Check(int.Parse(Scalar("SELECT COUNT(*) FROM Images")) == imagesBefore + (page == "24" && csrf == token ? 1 : 0), "image metadata unchanged on rejected upload");
                }
            }
            client.DefaultRequestHeaders.Add(AdminAntiforgery.HeaderName,token);
            try
            {
                await FileStatus(client,"POST","/admin","36",201,"Path",name,"Name","nested");
                Check(Directory.Exists(Path.Combine(folder,"nested")),"real folder created");
                await FileStatus(client,"POST","/admin","37",201,"Path",name,"Name","fixture");
                await FileStatus(client,"POST","/admin","38",201,"Path",name,"Name","fixture");
                await FileStatus(client,"POST","/admin","39",200,"Path",name+">fixture.css","StyleModel.FileName","fixture","StyleModel.FileContent","body { color: red; }");
                await FileStatus(client,"POST","/admin","40",200,"Path",name+">fixture.js","StyleModel.FileName","fixture","StyleModel.FileContent","var fixture = 233;");
                Check(File.ReadAllText(Path.Combine(folder,"fixture.css")).Contains("red"),"CSS edited");
                Check(File.ReadAllText(Path.Combine(folder,"fixture.js")).Contains("233"),"script edited");
                await FileStatus(client,"DELETE","/admin","41",200,"Path",name+">fixture.css");
                Check(!File.Exists(Path.Combine(folder,"fixture.css")),"real file deleted");
                await FileStatus(client,"DELETE","/admin","25",409,"itemID",productId.ToString(),"imageID","1");
                string html = await client.GetStringAsync("/admin?pageID=23&itemID=" + productId);
                string imageName = WebUtility.HtmlDecode(Regex.Match(html, "name=\"imageName\" value=\"([^\"]+)\"").Groups[1].Value);
                string imageToken = WebUtility.HtmlDecode(Regex.Match(html, "name=\"imageToken\" value=\"([^\"]+)\"").Groups[1].Value);
                Check(imageName == "csrf-fixture.jpg" && !string.IsNullOrEmpty(imageToken), "реальная Razor-форма содержит имя и отдельный токен набора");
                await FileStatus(client,"DELETE","/admin","25",200,"itemID",productId.ToString(),"imageName",imageName,"imageToken",imageToken);
                await FileStatus(client,"DELETE","/admin","25",409,"itemID",productId.ToString(),"imageName",imageName,"imageToken",imageToken);
                Check(!File.Exists(Path.Combine(productFolder,"csrf-fixture.jpg")) && Scalar("SELECT COUNT(*) FROM Images") == "0","real image and metadata deleted");
            }
            finally { client.DefaultRequestHeaders.Remove(AdminAntiforgery.HeaderName); }
            Console.WriteLine("File mutations: real PNG PUT 24/35, POST 36/37/38/39/40, DELETE 41/25 passed in owned isolated paths");
        }
        finally
        {
            Sql("DROP TEMPORARY TABLE IF EXISTS Images,ProductPages");
            // [Комментарий ИИ-агента]: Перед рекурсивным удалением проверяем абсолютные границы двух собственных фикстур; исходное Storage не удаляется.
            string bound = Path.GetFullPath(storage) + Path.DirectorySeparatorChar;
            foreach (string path in new[]{folder,productFolder})
            {
                if (!path.StartsWith(bound,StringComparison.OrdinalIgnoreCase) || path == bound) throw new InvalidOperationException("Unsafe fixture path");
                if (Directory.Exists(path)) Directory.Delete(path,true);
            }
        }
    }
}
