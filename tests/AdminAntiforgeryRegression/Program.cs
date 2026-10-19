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
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
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

internal static partial class AntiforgeryRegression
{
    static int checks, requests, serverRequests, probeRequests;
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
            // [Комментарий ИИ-агента]: Старые бизнес-регрессии теперь отправляют актуальный токен; отрицательные CSRF-проверки используют отдельный RawMutation без автоподстановки.
            if (client.BaseAddress.Scheme == "https" && !client.DefaultRequestHeaders.Contains(AdminAntiforgery.HeaderName)
                && !data.ContainsKey(AdminAntiforgery.FieldName))
                request.Headers.Add(AdminAntiforgery.HeaderName, await CurrentToken(client));
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

    static string receivedHeader;
    static void ClientTransport(string root, string token, CookieContainer jar, Uri origin)
    {
        var start = new System.Diagnostics.ProcessStartInfo("node",
            "\"" + Path.Combine(root, "tests/AdminAntiforgeryRegression/client.js") + "\" \"" + root + "\" --wire")
        {
            UseShellExecute = false, CreateNoWindow = true,
            RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true
        };
        using (var process = System.Diagnostics.Process.Start(start))
        {
            // [Комментарий ИИ-агента]: Токен и cookie передаются дочернему тесту только в памяти через stdin, без URL, файлов и аргументов процесса.
            process.StandardInput.Write(Newtonsoft.Json.JsonConvert.SerializeObject(new {
                token, cookie = jar.GetCookieHeader(origin), origin = origin.GetLeftPart(UriPartial.Authority)
            }));
            process.StandardInput.Close();
            string output = process.StandardOutput.ReadToEnd();
            string errors = process.StandardError.ReadToEnd();
            Check(process.WaitForExit(60000) && process.ExitCode == 0, "real JavaScript HTTPS transport regression");
            Check(output.Contains("\"wireRequests\":42"), "41 operation/method pairs and separate inline logout sent over HTTPS");
            Console.WriteLine("Client transport: " + output.Trim());
        }
    }
    static async Task<string> HtmlToken(HttpClient client, string path, bool login = false)
    {
        using (var response = await client.GetAsync(path))
        {
            Check(response.StatusCode == HttpStatusCode.OK, "real Razor HTML renders");
            Check(response.Headers.CacheControl.NoStore, "token HTML no-store");
            string html = await response.Content.ReadAsStringAsync();
            if (response.Headers.TryGetValues("Set-Cookie", out var cookies))
                foreach (string cookie in cookies.Where(c => c.StartsWith(AdminAntiforgery.CookieName + "=")))
                    Check(cookie.Contains("secure") && cookie.Contains("httponly") && cookie.Contains("samesite=lax")
                        && cookie.Contains("path=/") && !cookie.Contains("domain=") && !cookie.Contains("expires="), "antiforgery cookie flags and browser lifetime");
            string token = Regex.Match(html, "<meta name=\"csrf-token\" content=\"([^\"]+)\"").Groups[1].Value;
            Check(token.Length > 40, "HTML meta request token issued");
            int unsafeForms = 0;
            foreach (Match form in Regex.Matches(html, @"<form\b[^>]*>[\s\S]*?</form>"))
            {
                string markup = form.Value;
                bool post = Regex.IsMatch(markup, "^<form[^>]*method=\"post\"", RegexOptions.IgnoreCase);
                bool hidden = markup.Contains("name=\"__RequestVerificationToken\"");
                if (post) {
                    unsafeForms++;
                    Check(hidden, "rendered unsafe form contains hidden token");
                    Check(markup.Contains("value=\"" + token + "\""), "hidden token and meta share request context");
                }
                else Check(!hidden, "GET form excludes token");
            }
            Check(unsafeForms > 0, "rendered mutation forms found");
            Check(!Regex.IsMatch(html, "(?:href|action)=\"[^\"]*__RequestVerificationToken"), "no token in HTML URLs");
            if (login) Check(html.Contains("id=\"login-form\""), "expected login after revoked session");
            return WebUtility.HtmlDecode(token);
        }
    }
    static async Task<int> Probe(HttpClient client, string token, string method = "POST", bool hidden = false)
    {
        using (var request = new HttpRequestMessage(new HttpMethod(method), "/__csrf_probe"))
        using (var content = new MultipartFormDataContent())
        {
            content.Add(new StringContent("unchanged"), "payload");
            if (hidden) content.Add(new StringContent(token), AdminAntiforgery.FieldName);
            else if (token != null) request.Headers.Add(AdminAntiforgery.HeaderName, token);
            request.Content = content;
            using (var response = await client.SendAsync(request)) return (int)response.StatusCode;
        }
    }
    static async Task TokenStatus(HttpClient client, string token, string method, string page, int expected, params string[] fields)
    {
        client.DefaultRequestHeaders.Add(AdminAntiforgery.HeaderName, token);
        try {
            await Status(client, method, "/admin", page, expected, fields);
            Check(receivedHeader == token, "real MVC request receives exact CSRF header");
        }
        finally { client.DefaultRequestHeaders.Remove(AdminAntiforgery.HeaderName); }
    }
    static async Task TokenFlows(HttpClient client, CookieContainer jar, Uri origin, string root)
    {
        string anonymous = await HtmlToken(client, "/admin", true);
        var csrfCookie = jar.GetCookies(origin)[AdminAntiforgery.CookieName];
        Check(csrfCookie != null && csrfCookie.Secure && csrfCookie.HttpOnly && csrfCookie.Path == "/", "separate secure HttpOnly root cookie");
        string stableCookie = csrfCookie.Value;
        using (var response = await client.GetAsync("/private/control-room")) {
            Check(!response.Headers.Contains("Set-Cookie"), "existing antiforgery cookie not rotated between tabs");
        }
        Check(await Probe(client, anonymous) == 204, "anonymous pair accepted by stock antiforgery");
        Check(await Probe(client, anonymous, "POST", true) == 204, "native hidden token accepted");
        Check(await Probe(client, null) == 400 && await Probe(client, "broken") == 400, "test probe rejects missing and corrupt tokens");
        await TokenStatus(client, anonymous, "POST", "54", 401, "Login", "fixture", "Password", "wrong");
        Check(await Probe(client, anonymous) == 204, "failed login keeps context");
        await TokenStatus(client, anonymous, "POST", "54", 200, "Login", "fixture", "Password", password);
        string session = jar.GetCookies(origin)[SessionCookies.Name].Value;
        Check(await Probe(client, anonymous) == 400, "login invalidates anonymous request token for new session cookie");
        string tabA = await HtmlToken(client, "/admin?pageID=9");
        string tabB = await HtmlToken(client, "/private/control-room?pageID=9");
        ClientTransport(root, tabA, jar, origin);
        await UserValidationFlows(client, tabA);
        Check(await Probe(client, tabA) == 204 && await Probe(client, tabB) == 204, "multiple tab tokens coexist");
        var otherJar = new CookieContainer();
        otherJar.Add(origin, new Cookie(SessionCookies.Name, session, "/") { Secure = true });
        using (var other = new HttpClient(new HttpClientHandler {
            CookieContainer = otherJar, AllowAutoRedirect = false,
            // [Комментарий ИИ-агента]: Исключение проверки сертификата ограничено одноразовым локальным HTTPS-хостом теста.
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        }) { BaseAddress = origin }) {
            Check(await Probe(other, tabA) == 400, "request token without antiforgery cookie rejected");
            await HtmlToken(other, "/admin?pageID=9");
            Check(await Probe(other, tabA) == 400, "another browser cookie cannot reuse token even with same session");
        }
        Check(await Probe(client, tabA, "PUT") == 204 && await Probe(client, tabA, "DELETE") == 204, "real multipart PUT DELETE headers accepted");
        Check(jar.GetCookies(origin)[AdminAntiforgery.CookieName].Value == stableCookie, "no antiforgery cookie rotation on login");
        using (var response = await client.GetAsync("/admin?pageID=9")) {
            string html = await response.Content.ReadAsStringAsync();
            SessionToken.TryHash(session, out string hash);
            Check(!html.Contains(session) && !html.Contains(hash), "session token and hash not disclosed in HTML");
        }
        Sql("UPDATE UserTypes SET AccessLevel=1 WHERE ID=1");
        var matrix = new Dictionary<string,string[]> {
            {"POST",new[]{"13","14","16","18","20","21","26","27","29","30","32","33","36","37","38","39","40","42","43","46","47","49","50","52","53","59"}},
            {"PUT",new[]{"24","35"}}, {"DELETE",new[]{"15","17","22","25","28","31","34","41","44","48","51"}}
        };
        foreach (var pair in matrix) foreach (string page in pair.Value)
            await TokenStatus(client, tabA, pair.Key, page, 403);
        Sql("UPDATE UserTypes SET AccessLevel=5 WHERE ID=1");
        await TokenStatus(client, tabA, "POST", "52", 200, "UserModel.ID", "1", "UserModel.Login", "fixture", "UserModel.CurrentPassword", password, "UserModel.IdleTime", "10");
        Check(await Probe(client, tabB) == 204, "profile without password preserves context");
        await HtmlToken(client, "/admin?pageID=11");
        await TokenStatus(client, tabB, "DELETE", "58", 200);
        await TokenStatus(client, tabA, "POST", "46", 401);
        Check(await Probe(client, tabA) == 204, "D09 residual cookie pair stays valid after logout; DB denies access");
        string loggedOut = await HtmlToken(client, "/admin", true);
        await TokenStatus(client, loggedOut, "DELETE", "58", 200);
        await TokenStatus(client, loggedOut, "POST", "54", 200, "Login", "fixture", "Password", password);
        Check(await Probe(client, tabA) == 400 && await Probe(client, tabB) == 400, "new login invalidates both old tab contexts");
        string current = await HtmlToken(client, "/admin?pageID=9");
        string changed = "Csrf-changed-2026!";
        await TokenStatus(client, current, "POST", "52", 200, "UserModel.ID", "1", "UserModel.Login", "fixture", "UserModel.CurrentPassword", password, "UserModel.NewPassword", changed, "UserModel.IdleTime", "10");
        Check(Scalar("SELECT COUNT(*) FROM ConnectedUsers") == "0", "ordinary password revokes sessions");
        await TokenStatus(client, current, "POST", "46", 401);
        await HtmlToken(client, "/admin", true);
        Check(await Probe(client, current) == 204, "password revocation preserves D09 cookie context");
        await TokenStatus(client, current, "POST", "54", 200, "Login", "fixture", "Password", changed);
        Check(await Probe(client, current) == 400, "password relogin changes context");
        Sql("UPDATE Users SET RequirePasswordChange=1 WHERE ID=1");
        string initial = await HtmlToken(client, "/private/control-room");
        client.DefaultRequestHeaders.Accept.ParseAdd("text/html");
        using (var response = await Send(client, "POST", "/private/control-room", "59", AdminAntiforgery.FieldName, initial,
            "UserModel.CurrentPassword", changed, "UserModel.NewPassword", password, "UserModel.ConfirmPassword", password)) {
            Check(response.StatusCode == HttpStatusCode.Redirect, "initial password keeps HTML redirect");
            Check(string.IsNullOrEmpty(receivedHeader), "ordinary password form sends hidden token without AJAX header");
        }
        client.DefaultRequestHeaders.Accept.Clear();
        await HtmlToken(client, "/private/control-room", true);
        Check(Scalar("SELECT COUNT(*) FROM ConnectedUsers") == "0", "initial password revokes sessions");
        Check(jar.GetCookies(origin)[AdminAntiforgery.CookieName].Value == stableCookie, "no cookie races on logout/password");
        // [Комментарий ИИ-агента]: Повторный вход после HTML GET использует актуальную пару токенов.
        await Status(client, "POST", "/admin", "54", 200, "Login", "fixture", "Password", password);
        await Status(client, "DELETE", "/admin", "58", 200);
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
                .UseContentRoot(Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../StoreCMS")))
                .UseSetting(WebHostDefaults.ApplicationKey, typeof(AdminPanelController).Assembly.GetName().Name)
                .ConfigureServices(services =>
                {
                    services.AddDataProtection().UseEphemeralDataProtectionProvider();
                    AdminAntiforgery.Configure(services);
                    services.AddTransient<ILoginFormLocalization>(_ => new RuLoginFormLocalization());
                    services.AddTransient<IAdminPanelPageLocalization>(_ => new RuAdminPanelPageLocalization());
                    services.AddTransient<IUserTypesLocalization>(_ => new RuUserTypesLocalization());
                    services.AddTransient<IUserProfileLocalization>(_ => new RuUserProfileLocalization());
                    services.AddTransient<IUsersLocalization>(_ => new RuUsersLocalization());
                    services.AddTransient<ICategoriesAndProductsLocalization>(_ => new RuCategoriesAndProductsLocalization());
                    services.AddSingleton<Treynessen.Translit.EnRuTranslit>();
                    services.AddSingleton(options);
                    services.AddScoped<CMSDatabase>();
                    services.AddTransient<IAdminPanelLogLocalization>(_ => new RuAdminPanelLogLocalization());
                    var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>()).Build();
                    services.AddTransient(_ => new AccessLevelConfiguration(config.GetSection("Access")));
                    services.AddMvc(mvc => { mvc.Filters.Add(new MutationBoundaryFilter()); mvc.Filters.Add(new PrgResultFilter()); }).AddApplicationPart(typeof(AdminPanelController).Assembly);
                })
                .Configure(app => {
                    app.UsePathBase("/audit-base");
                    // [Комментарий ИИ-агента]: Проба отдельно проверяет криптографию; реальная обязательная проверка находится в фильтре production-контроллера.
                    app.Use(async (context, next) => {
                        System.Threading.Interlocked.Increment(ref serverRequests);
                        if (context.Request.Path == "/__csrf_probe") {
                            System.Threading.Interlocked.Increment(ref probeRequests);
                            try {
                                await context.RequestServices.GetRequiredService<IAntiforgery>().ValidateRequestAsync(context);
                                var form = await context.Request.ReadFormAsync();
                                context.Response.StatusCode = form["payload"] == "unchanged" ? 204 : 422;
                            }
                            catch (AntiforgeryValidationException) { context.Response.StatusCode = 400; }
                            return;
                        }
                        receivedHeader = context.Request.Headers[AdminAntiforgery.HeaderName];
                        await next();
                    });
                    app.UseMvc(routes =>
                {
                    routes.MapRoute("admin", "admin", new { controller = "AdminPanel", action = "AdminPanel" });
                    routes.MapRoute("custom", "private/control-room", new { controller = "AdminPanel", action = "AdminPanel" });
                    routes.MapRoute("escaped", "private/space room", new { controller = "AdminPanel", action = "AdminPanel" });
                }); }).Build())
            {
                host.Start();
                var addresses = host.ServerFeatures.Get<IServerAddressesFeature>().Addresses;
                var origin = new Uri(addresses.Single(a => a.StartsWith("https://", StringComparison.Ordinal)));
                using (var plainClient = new HttpClient { BaseAddress = new Uri(addresses.Single(a => a.StartsWith("http://", StringComparison.Ordinal))) }) {
                    using (var response = await plainClient.GetAsync("/admin")) {
                        Check(response.StatusCode == HttpStatusCode.Forbidden, "HTTP cannot render token HTML");
                        Check(!response.Headers.Contains("Set-Cookie"), "HTTP does not issue antiforgery cookie");
                    }
                    foreach (string path in new[] { "/admin", "/private/control-room" })
                        foreach (string method in new[] { "POST", "PUT", "DELETE" })
                            await Status(plainClient, method, path, "999", 403);
                }
                var jar = new CookieContainer();
                using (var client = new HttpClient(new HttpClientHandler
                {
                    CookieContainer = jar, AllowAutoRedirect = false,
                    // [Комментарий ИИ-агента]: Одноразовый сертификат принимается только локальным тестовым клиентом.
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                }) { BaseAddress = origin })
                {
                    await EnforcementFlows(client, jar, origin);
                    await NativePrgFlows(client, jar);
                    await TokenFlows(client, jar, origin, root);
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
            expectF02 = Array.IndexOf(args, "--expect-f02") >= 0;
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
            Console.WriteLine("HTTP assertions: " + checks + "; dispatch requests: " + requests
                + "; all server requests: " + serverRequests + "; probe requests: " + probeRequests); return 0;
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
