using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MySql.Data.MySqlClient;
using Treynessen.AdminPanelTypes;
using Treynessen.Controllers;
using Treynessen.Database.Context;
using Treynessen.Localization;
using Treynessen.Security;

internal static class HttpErrorRegression
{
    static int checks, requests;
    static MySqlConnection connection;
    static DbContextOptions<CMSDatabase> options;
    static readonly string password = "Http-error-fixture-2026!";
    static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException("CHECK: " + message);
        checks++;
    }
    static void Sql(string sql)
    {
        using (var command = new MySqlCommand(sql, connection)) command.ExecuteNonQuery();
    }
    static string Tables()
    {
        using (var command = new MySqlCommand("SELECT GROUP_CONCAT(TABLE_NAME ORDER BY TABLE_NAME) FROM information_schema.tables WHERE TABLE_SCHEMA=DATABASE()", connection))
            return Convert.ToString(command.ExecuteScalar());
    }
    static string Scalar(string sql)
    {
        using (var command = new MySqlCommand(sql, connection)) return Convert.ToString(command.ExecuteScalar());
    }
    static void Fixtures()
    {
        // [Комментарий ИИ-агента]: Только временные таблицы одного открытого соединения скрывают постоянные таблицы test; Program и EnsureCreated приложения не запускаются.
        Sql("CREATE TEMPORARY TABLE UserTypes(ID int NOT NULL AUTO_INCREMENT PRIMARY KEY,Name varchar(255) NOT NULL,AccessLevel int NOT NULL) ENGINE=InnoDB");
        Sql("CREATE TEMPORARY TABLE Users(ID int NOT NULL AUTO_INCREMENT PRIMARY KEY,Login varchar(255) NOT NULL,Password longtext NOT NULL,RequirePasswordChange tinyint(1) NOT NULL,Email longtext NULL,IdleTime int unsigned NOT NULL,UserTypeID int NOT NULL) ENGINE=InnoDB");
        Sql("CREATE TEMPORARY TABLE ConnectedUsers(ID int NOT NULL AUTO_INCREMENT PRIMARY KEY,TokenHash char(64) CHARACTER SET ascii COLLATE ascii_bin NOT NULL UNIQUE,LastActionUtc datetime(6) NOT NULL,IPAdress varchar(45) NOT NULL,UserAgent varchar(255) NOT NULL,UserID int NOT NULL UNIQUE) ENGINE=InnoDB");
        Sql("CREATE TEMPORARY TABLE AdminPanelLogs(ID int NOT NULL AUTO_INCREMENT PRIMARY KEY,Info varchar(255) NOT NULL,Time datetime(6) NOT NULL,UserId int NOT NULL) ENGINE=InnoDB");
        Sql("INSERT INTO UserTypes VALUES(1,'fixture',5)");
        using (var command = new MySqlCommand("INSERT INTO Users VALUES(1,'fixture',@hash,0,NULL,10,1)", connection))
        {
            command.Parameters.AddWithValue("@hash", SecurityFunctions.GetPasswordHash(password));
            command.ExecuteNonQuery();
        }
    }
    static X509Certificate2 Certificate()
    {
        using (var rsa = RSA.Create(2048))
        {
            var request = new CertificateRequest("CN=localhost", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            using (var certificate = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddHours(1)))
                return new X509Certificate2(certificate.Export(X509ContentType.Pfx));
        }
    }
    static async Task<HttpResponseMessage> Send(HttpClient client, string method, string path, string page, params string[] fields)
    {
        var data = new Dictionary<string, string>();
        if (page != null) data.Add("pageID", page);
        for (int i = 0; i < fields.Length; i += 2) data.Add(fields[i], fields[i + 1]);
        using (var request = new HttpRequestMessage(new HttpMethod(method), path))
        {
            request.Content = new FormUrlEncodedContent(data);
            requests++;
            return await client.SendAsync(request);
        }
    }
    static async Task Status(HttpClient client, string method, string path, string page, int expected, params string[] fields)
    {
        using (var response = await Send(client, method, path, page, fields))
        {
            Check((int)response.StatusCode == expected, method + " " + page + " expected " + expected + " actual " + (int)response.StatusCode);
            Check(response.Headers.Location == null, "failure and plain status responses do not redirect");
            Check(response.Headers.CacheControl != null && response.Headers.CacheControl.NoStore, "no-store retained");
        }
    }
    static async Task InvalidSelectors(HttpClient client, int expected)
    {
        foreach (string path in new[] { "/admin", "/private/control-room" })
            foreach (string method in new[] { "POST", "PUT", "DELETE" })
                foreach (string page in new[] { "999", "-1", "60", "0", "2147483648", null, "", "not-a-page" })
                    await Status(client, method, path, page, expected);
    }
    static async Task UnsupportedMethods(HttpClient client)
    {
        foreach (string path in new[] { "/admin", "/private/control-room" })
        {
            await Status(client, "POST", path, "35", 400);
            await Status(client, "PUT", path, "46", 400);
            await Status(client, "DELETE", path, "46", 400);
            foreach (string method in new[] { "POST", "PUT", "DELETE" })
                await Status(client, method, path, "1", 400);
            await Status(client, "PUT", path, "54", 400);
            await Status(client, "DELETE", path, "54", 400);
            await Status(client, "POST", path, "58", 400);
            await Status(client, "PUT", path, "58", 400);
            await Status(client, "PUT", path, "59", 400);
            await Status(client, "DELETE", path, "59", 400);
        }
    }
    static async Task SpecialFlows(HttpClient client)
    {
        string currentPassword = password;
        int iteration = 0;
        foreach (string path in new[] { "/admin", "/private/control-room" })
        {
            Sql("UPDATE Users SET RequirePasswordChange=1 WHERE ID=1");
            await Status(client, "POST", path, "46", 403);
            await Status(client, "POST", path, "54", 403, "Login", "fixture", "Password", currentPassword);
            foreach (string method in new[] { "POST", "PUT", "DELETE" })
                await Status(client, method, path, "999", 400);
            string nextPassword = "Updated-http-fixture-2026-" + (++iteration) + "!";
            using (var response = await Send(client, "POST", path, "59", "UserModel.CurrentPassword", currentPassword,
                "UserModel.NewPassword", nextPassword, "UserModel.ConfirmPassword", nextPassword))
            {
                Check(response.StatusCode == HttpStatusCode.Redirect, "initial password HTML success keeps redirect");
                Check(response.Headers.Location != null && response.Headers.Location.OriginalString == path, "only intentional local redirect on password success");
                Check(response.Headers.CacheControl != null && response.Headers.CacheControl.NoStore, "password success no-store");
            }
            Check(Scalar("SELECT COUNT(*) FROM ConnectedUsers") == "0", "password change revokes session");
            Check(Scalar("SELECT CAST(RequirePasswordChange AS UNSIGNED) FROM Users WHERE ID=1") == "0", "password change clears forced flag");
            await Status(client, "POST", path, "46", 401);
            await Status(client, "POST", path, "54", 401, "Login", "fixture", "Password", currentPassword);
            currentPassword = nextPassword;
            await Status(client, "POST", path, "54", 200, "Login", "fixture", "Password", currentPassword);
            Sql("UPDATE Users SET RequirePasswordChange=1 WHERE ID=1");
            await Status(client, "DELETE", path, "58", 200);
            await Status(client, "DELETE", path, "58", 200);
            await Status(client, "POST", path, "59", 401);
            Sql("UPDATE Users SET RequirePasswordChange=0 WHERE ID=1");
            await Status(client, "POST", path, "54", 200, "Login", "fixture", "Password", currentPassword);
        }
    }
    static async Task Run(string root)
    {
        string before = Tables();
        Fixtures();
        try
        {
            using (var certificate = Certificate())
            using (var host = new WebHostBuilder()
                .UseKestrel(k =>
                {
                    k.Listen(IPAddress.Loopback, 0, listen => listen.UseHttps(certificate));
                    k.Listen(IPAddress.Loopback, 0);
                })
                .ConfigureServices(services =>
                {
                    services.AddSingleton(options);
                    services.AddScoped<CMSDatabase>();
                    services.AddTransient<IAdminPanelLogLocalization>(_ => new RuAdminPanelLogLocalization());
                    var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>()).Build();
                    services.AddTransient(_ => new AccessLevelConfiguration(config.GetSection("Access")));
                    services.AddMvc().AddApplicationPart(typeof(AdminPanelController).Assembly);
                })
                .Configure(app => app.UseMvc(routes =>
                {
                    routes.MapRoute("admin", "admin", new { controller = "AdminPanel", action = "AdminPanel" });
                    routes.MapRoute("custom", "private/control-room", new { controller = "AdminPanel", action = "AdminPanel" });
                })).Build())
            {
                host.Start();
                var addresses = host.ServerFeatures.Get<IServerAddressesFeature>().Addresses;
                var origin = new Uri(addresses.Single(a => a.StartsWith("https://", StringComparison.Ordinal)));
                using (var plainClient = new HttpClient { BaseAddress = new Uri(addresses.Single(a => a.StartsWith("http://", StringComparison.Ordinal))) })
                    foreach (string path in new[] { "/admin", "/private/control-room" })
                        foreach (string method in new[] { "POST", "PUT", "DELETE" })
                            await Status(plainClient, method, path, "999", 403);
                var jar = new CookieContainer();
                using (var client = new HttpClient(new HttpClientHandler
                {
                    CookieContainer = jar, AllowAutoRedirect = false,
                    // [Комментарий ИИ-агента]: Одноразовый сертификат принимается только локальным тестовым клиентом.
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                }) { BaseAddress = origin })
                {
                    var operations = new Dictionary<string, string[]> {
                        {"POST", new[] {"13","14","16","18","20","21","26","27","29","30","32","33","36","37","38","39","40","42","43","46","47","49","50","52","53","59"}},
                        {"PUT", new[] {"24","35"}},
                        {"DELETE", new[] {"15","17","22","25","28","31","34","41","44","48","51"}}
                    };
                    foreach (var path in new[] { "/admin", "/private/control-room" })
                    {
                        foreach (var pair in operations)
                            foreach (string page in pair.Value) await Status(client, pair.Key, path, page, 401);
                        await Status(client, "DELETE", path, "58", 200);
                    }
                    await InvalidSelectors(client, 401);
                    await Status(client, "POST", "/admin", "54", 401, "Login", "fixture", "Password", "wrong");
                    await Status(client, "POST", "/admin", "54", 200, "Login", "fixture", "Password", password);
                    string cookie = jar.GetCookies(origin)[SessionCookies.Name].Value;
                    Check(!string.IsNullOrEmpty(cookie), "real controller login issues session v2 cookie");
                    Sql("UPDATE UserTypes SET AccessLevel=1 WHERE ID=1");
                    await InvalidSelectors(client, 400);
                    await UnsupportedMethods(client);
                    foreach (var path in new[] { "/admin", "/private/control-room" })
                    {
                        foreach (var pair in operations)
                            foreach (string page in pair.Value.Where(p => p != "59")) await Status(client, pair.Key, path, page, 403);
                    }
                    Sql("UPDATE UserTypes SET AccessLevel=5 WHERE ID=1");
                    await InvalidSelectors(client, 400);
                    await UnsupportedMethods(client);
                    foreach (var method in new[] { "POST", "PUT", "DELETE" }) await Status(client, method, "/admin", "1", 400);
                    await Status(client, "POST", "/admin", "46", 422, "UserTypeModel.Name", "invalid", "UserTypeModel.AccessLevel", "0");
                    await Status(client, "POST", "/admin", "46", 201, "UserTypeModel.Name", "created-fixture", "UserTypeModel.AccessLevel", "2");
                    Check(Scalar("SELECT COUNT(*) FROM UserTypes WHERE Name='created-fixture'") == "1", "201 confirms actual isolated creation");
                    await Status(client, "POST", "/admin", "43", 200, "itemID", "1", "UserModel.UserTypeId", "1");
                    await Status(client, "POST", "/admin", "42", 409, "UserModel.Login", "fixture", "UserModel.NewPassword", password, "UserModel.UserTypeId", "1");
                    await Status(client, "DELETE", "/admin", "44", 404, "itemID", "1");
                    await Status(client, "DELETE", "/admin", "48", 404, "itemID", "999");
                    await Status(client, "PUT", "/admin", "35", 415, "path", "!!!");
                    Sql("UPDATE Users SET RequirePasswordChange=1 WHERE ID=1");
                    await Status(client, "POST", "/admin", "46", 403);
                    Sql("UPDATE Users SET RequirePasswordChange=0 WHERE ID=1");
                    await SpecialFlows(client);
                    Sql("UPDATE ConnectedUsers SET LastActionUtc=UTC_TIMESTAMP()-INTERVAL 11 MINUTE");
                    await Status(client, "POST", "/admin", "53", 401);
                    await Status(client, "DELETE", "/admin", "58", 200);
                    await Status(client, "DELETE", "/admin", "58", 200);
                }
                await host.StopAsync();
            }
        }
        finally
        {
            // [Комментарий ИИ-агента]: DROP TEMPORARY никогда не удаляет одноимённые постоянные таблицы; закрытие соединения также удаляет временные фикстуры.
            Sql("DROP TEMPORARY TABLE IF EXISTS AdminPanelLogs,ConnectedUsers,Users,UserTypes");
        }
        Check(before == Tables(), "permanent table inventory preserved");
    }
    public static int Main(string[] args)
    {
        try
        {
            string root = Path.GetFullPath(args[0]);
            // [Комментарий ИИ-агента]: Секрет читается только в память из разрешённых настроек AGENTS.md и не выводится в диагностику.
            string agents = File.ReadAllText(Path.Combine(root, "AGENTS.md"));
            string secret = Regex.Match(agents, @"\| Пароль \| `([^`]+)`").Groups[1].Value;
            if (string.IsNullOrEmpty(secret)) throw new InvalidOperationException("Fixture credentials missing");
            using (connection = new MySqlConnection(new MySqlConnectionStringBuilder
            {
                Server = "127.0.0.1", Port = 3306, Database = "test", UserID = "root", Password = secret,
                SslMode = MySqlSslMode.Required, Pooling = false, ConnectionTimeout = 5
            }.ConnectionString))
            {
                connection.Open();
                using (var command = new MySqlCommand("SELECT DATABASE()", connection))
                    Check(Convert.ToString(command.ExecuteScalar()) == "test", "read-only preflight existing test");
                options = new DbContextOptionsBuilder<CMSDatabase>().UseMySql(connection).Options;
                Run(root).GetAwaiter().GetResult();
            }
            Console.WriteLine("HTTP assertions: " + checks + "; HTTP requests: " + requests); return 0;
        }
        catch (Exception error)
        {
            Console.Error.WriteLine("FAILED: " + error.GetType().Name);
            if (error is MySqlException mysql) Console.Error.WriteLine("MySQL number: " + mysql.Number);
            if (error is InvalidOperationException && error.Message.StartsWith("CHECK: ")) Console.Error.WriteLine(error.Message);
            Console.Error.WriteLine("HTTP assertions completed: " + checks); return 1;
        }
    }
}
