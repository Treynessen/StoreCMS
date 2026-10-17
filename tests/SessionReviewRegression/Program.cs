using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using Treynessen.AdminPanelTypes;
using Treynessen.Database;
using Treynessen.Database.Context;
using Treynessen.Security;

internal static class Regression
{
    const string Password = "Review-temp-password!", NewPassword = "Review-new-password!";
    static MySqlConnection connection;
    static DbContextOptions<CMSDatabase> options;
    static readonly object databaseGate = new object();
    static int checks, auditErrors;
    static DateTime? clock;
    static TaskCompletionSource<bool> reached, release;
    static Uri origin;

    static void Check(bool condition, string label)
    {
        if (!condition) throw new InvalidOperationException("CHECK: " + label);
        checks++; Console.WriteLine("PASS: " + label);
    }
    static void Sql(string sql) { using (var command = new MySqlCommand(sql, connection)) command.ExecuteNonQuery(); }
    static string Scalar(string sql) { using (var command = new MySqlCommand(sql, connection)) return Convert.ToString(command.ExecuteScalar()); }
    static long Count(string sql) => long.Parse(Scalar(sql));
    static DateTime Last() { using (var db = new CMSDatabase(options)) return db.ConnectedUsers.AsNoTracking().Single().LastActionUtc; }
    static void Reset()
    {
        clock = null;
        // [Комментарий ИИ-агента]: TEMPORARY-таблицы доступны только этому подключению; одноимённые постоянные таблицы test не изменяются.
        Sql("DROP TEMPORARY TABLE IF EXISTS AdminPanelLogs,ConnectedUsers,Users,UserTypes");
        Sql("CREATE TEMPORARY TABLE UserTypes(ID int NOT NULL AUTO_INCREMENT PRIMARY KEY,Name varchar(255) NOT NULL,AccessLevel int NOT NULL) ENGINE=InnoDB");
        Sql("CREATE TEMPORARY TABLE Users(ID int NOT NULL AUTO_INCREMENT PRIMARY KEY,Login varchar(255) NOT NULL,Password longtext NOT NULL,RequirePasswordChange tinyint(1) NOT NULL,Email longtext NULL,IdleTime int unsigned NOT NULL,UserTypeID int NOT NULL) ENGINE=InnoDB");
        Sql("CREATE TEMPORARY TABLE ConnectedUsers(ID int NOT NULL AUTO_INCREMENT PRIMARY KEY,TokenHash char(64) CHARACTER SET ascii COLLATE ascii_bin NOT NULL UNIQUE,LastActionUtc datetime(6) NOT NULL,IPAdress varchar(45) NOT NULL,UserAgent varchar(255) NOT NULL,UserID int NOT NULL UNIQUE) ENGINE=InnoDB");
        Sql("CREATE TEMPORARY TABLE AdminPanelLogs(ID int NOT NULL AUTO_INCREMENT PRIMARY KEY,Info varchar(255) NOT NULL,Time datetime(6) NOT NULL,UserId int NOT NULL) ENGINE=InnoDB");
        Sql("INSERT INTO UserTypes VALUES(1,'review',5)");
        using (var command = new MySqlCommand("INSERT INTO Users VALUES(1,'review',@hash,0,NULL,10,1)", connection))
        { command.Parameters.AddWithValue("@hash", SecurityFunctions.GetPasswordHash(Password)); command.ExecuteNonQuery(); }
    }

    sealed class AuditLogger : ILoggerProvider, ILogger
    {
        public ILogger CreateLogger(string categoryName) => this;
        public void Dispose() { }
        public IDisposable BeginScope<TState>(TState state) => null;
        public bool IsEnabled(LogLevel level) => true;
        public void Log<TState>(LogLevel level, EventId id, TState state, Exception error, Func<TState, Exception, string> formatter)
        { if (id.Id == 2202) { Check(error == null, "audit diagnostic excludes provider exception"); auditErrors++; } }
    }

    static async Task Handle(HttpContext context)
    {
        var form = context.Request.Method == "POST" ? await context.Request.ReadFormAsync() : null;
        int status = 200;
        // [Комментарий ИИ-агента]: Запросы выполняют SQL последовательно на одном соединении; задерживается только отправка уже подготовленного ответа.
        lock (databaseGate)
        using (var db = new CMSDatabase(options))
        {
            try
            {
                if (context.Request.Path == "/login")
                {
                    if (!SecurityFunctions.IsValidLoginFormData(db, new LoginFormModel { Login = "review", Password = form["password"] }, context, out var user)) status = 403;
                    else status = DatabaseInteraction.AddConnectedUser(db, user, context) ? 200 : 409;
                }
                else if (context.Request.Path == "/logout") DatabaseInteraction.Exit(db, null, context, out status);
                else
                {
                    var user = SessionStore.Authenticate(db, context, AdminSessionPolicy.Instance, () => clock ?? DateTime.UtcNow);
                    context.Items["User"] = user;
                    if (user == null) status = 401;
                    else if (context.Request.Path == "/profile" || context.Request.Path == "/initial")
                    {
                        var model = new UserModel { ID = user.ID, Login = "review", IdleTime = 10,
                            CurrentPassword = form["password"], NewPassword = NewPassword, ConfirmPassword = NewPassword };
                        if (context.Request.Path == "/profile") DatabaseInteraction.EditUserData(db, model, context, out status);
                        else
                        {
                            DatabaseInteraction.ChangeInitialPassword(db, user, model, out status, context);
                            if (status == 200) SessionCookies.ClearLegacy(context);
                        }
                    }
                }
                Check(!db.ChangeTracker.Entries<Treynessen.Database.Entities.AdminPanelLog>().Any(e => e.State == EntityState.Added), "request leaves no failed pending audit entry");
            }
            catch (Exception error)
            {
                var failure = new ExceptionContext(new ActionContext(context, new RouteData(), new ActionDescriptor()), new List<IFilterMetadata>()) { Exception = error };
                new SessionExceptionFilter().OnException(failure);
                if (!failure.ExceptionHandled) throw;
                status = ((ObjectResult)failure.Result).StatusCode.Value;
            }
        }
        if (context.Request.Query["hold"] == "1")
        {
            reached.TrySetResult(true);
            await release.Task;
        }
        context.Response.StatusCode = status;
        await context.Response.WriteAsync("review fixture");
    }

    static HttpClient Client(out CookieContainer jar)
    {
        jar = new CookieContainer();
        return new HttpClient(new HttpClientHandler { CookieContainer = jar, AllowAutoRedirect = false,
            ServerCertificateCustomValidationCallback = (message, certificate, chain, errors) => true }) { BaseAddress = origin, Timeout = TimeSpan.FromSeconds(20) };
    }
    static string Cookie(CookieContainer jar) => jar.GetCookies(origin)[SessionCookies.Name]?.Value;
    static Task<HttpResponseMessage> Post(HttpClient client, string path, string password = Password) => client.PostAsync(path,
        new FormUrlEncodedContent(new Dictionary<string, string> { { "password", password } }));
    static async Task Login(HttpClient client, string password = Password)
    {
        using (var response = await Post(client, "/login", password))
        {
            Check(response.StatusCode == HttpStatusCode.OK, "HTTP login succeeds");
            string header = response.Headers.GetValues("Set-Cookie").Single(h => h.StartsWith(SessionCookies.Name + "="));
            Check(header.Contains("secure") && header.Contains("httponly") && header.Contains("samesite=lax"), "secure HttpOnly SameSite serialized by 2.2 runtime");
        }
    }
    static async Task Denied(string token)
    {
        using (var client = Client(out var jar))
        {
            jar.SetCookies(origin, SessionCookies.Name + "=" + token + "; path=/; secure");
            using (var response = await client.GetAsync("/auth")) Check(response.StatusCode == HttpStatusCode.Unauthorized, "old token remains denied over HTTP");
        }
    }
    static async Task OrderedResponse(string path, bool replacedBeforeRequest = false, bool initial = false, bool expired = false)
    {
        Reset();
        if (initial) Sql("UPDATE Users SET RequirePasswordChange=1");
        using (var client = Client(out var jar))
        {
            await Login(client); string old = Cookie(jar);
            if (expired) clock = Last().AddMinutes(10);
            if (replacedBeforeRequest)
            {
                await Login(client);
                // [Комментарий ИИ-агента]: Отдельный старый запрос использует A, а его ответ будет применён к тому же хранилищу после выдачи B.
                jar.SetCookies(origin, SessionCookies.Name + "=" + old + "; path=/; secure");
            }
            reached = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            Task<HttpResponseMessage> pending = path == "/profile" || path == "/initial"
                ? Post(client, path + "?hold=1") : client.GetAsync(path + "?hold=1");
            try
            {
                Check(await Task.WhenAny(reached.Task, Task.Delay(10000)) == reached.Task, "old request completed DB operation before response gate: " + path);
                if (path == "/logout" && !replacedBeforeRequest || path == "/profile" || path == "/initial")
                    Check(Count("SELECT COUNT(*) FROM ConnectedUsers") == 0, "revocation committed before delayed response");
                string loginPassword = path == "/profile" || path == "/initial" ? NewPassword : Password;
                clock = null;
                await Login(client, loginPassword); string current = Cookie(jar);
                Check(current != old && current != null, "replacement cookie B received before delayed response");
                release.TrySetResult(true);
                using (var response = await pending)
                {
                    Check(response.StatusCode == (path == "/auth" ? HttpStatusCode.Unauthorized : HttpStatusCode.OK), "delayed operation has correct HTTP status");
                    Check(!response.Headers.TryGetValues("Set-Cookie", out var headers) || !headers.Any(h => h.StartsWith(SessionCookies.Name + "=")), "delayed response never expires shared auth cookie");
                }
                Check(Cookie(jar) == current, "final cookie jar retains B after actual reordered HTTP responses: " + path);
                Check(Count("SELECT COUNT(*) FROM ConnectedUsers") == 1, "only replacement server session survives");
                using (var response = await client.GetAsync("/auth")) Check(response.StatusCode == HttpStatusCode.OK, "next HTTP request with final jar authenticates");
                await Denied(old);
            }
            finally { release.TrySetResult(true); }
        }
    }

    static async Task OrdinaryLogout()
    {
        Reset();
        using (var client = Client(out var jar))
        {
            await Login(client); string old = Cookie(jar);
            using (var response = await client.GetAsync("/logout")) Check(response.StatusCode == HttpStatusCode.OK, "explicit logout succeeds");
            Check(Count("SELECT COUNT(*) FROM ConnectedUsers") == 0, "explicit logout removes server session");
            Check(Cookie(jar) == old, "residual revoked cookie is deliberately retained");
            await Denied(old);
            using (var response = await client.GetAsync("/logout")) Check(response.StatusCode == HttpStatusCode.OK, "repeated logout is idempotent");
        }
    }

    static async Task AuditFailure()
    {
        Reset(); int before = auditErrors;
        Sql("ALTER TABLE AdminPanelLogs ADD UNIQUE INDEX duplicate_info(Info)");
        Sql("INSERT INTO AdminPanelLogs(Info,Time,UserId) VALUES('. IP: 127.0.0.1',UTC_TIMESTAMP(6),1),('',UTC_TIMESTAMP(6),1)");
        using (var client = Client(out var jar))
        {
            await Login(client); string old = Cookie(jar);
            Check(auditErrors == before + 1 && Count("SELECT COUNT(*) FROM ConnectedUsers") == 1, "audit INSERT fails after login commit; diagnostic emitted, session committed");
            using (var response = await client.GetAsync("/auth")) Check(response.StatusCode == HttpStatusCode.OK, "successful login credential works despite audit failure");
            using (var response = await Post(client, "/profile")) Check(response.StatusCode == HttpStatusCode.OK, "password operation returns HTTP 200 despite post-commit audit failure");
            Check(auditErrors == before + 2 && Count("SELECT COUNT(*) FROM AdminPanelLogs") == 2, "both audit writes failed without inserting duplicate events");
            using (var db = new CMSDatabase(options)) Check(SecurityFunctions.VerifyPassword(db.Users.Single(), NewPassword) && !SecurityFunctions.VerifyPassword(db.Users.Single(), Password), "new password committed, old password rejected");
            Check(Count("SELECT COUNT(*) FROM ConnectedUsers") == 0, "password and revocation remain committed together");
            await Denied(old); await Login(client, NewPassword);
        }
    }

    static async Task JournalIdle()
    {
        Reset();
        using (var client = Client(out var jar))
        {
            await Login(client); DateTime start = Last();
            clock = start.AddMinutes(2);
            using (var response = await client.GetAsync("/open")) Check(response.StatusCode == HttpStatusCode.OK && Last() == clock.Value, "opening profile renews idle UTC");
            clock = start.AddMinutes(3);
            using (var response = await client.GetAsync("/journal?pageID=" + (int)AdminPanelPages.GetUserLog)) Check(response.StatusCode == HttpStatusCode.OK && Last() == clock.Value, "one automatic journal child GET renews as part of page opening");
            clock = start.AddMinutes(4);
            using (var response = await client.GetAsync("/journal?pageID=" + (int)AdminPanelPages.GetUserLog)) Check(response.StatusCode == HttpStatusCode.OK && Last() == clock.Value, "manual journal GET still renews idle");
            clock = start.AddMinutes(5);
            using (var request = new HttpRequestMessage(HttpMethod.Get, "/journal"))
            {
                request.Headers.Add("Sec-Purpose", "prefetch");
                using (var response = await client.SendAsync(request)) Check(response.StatusCode == HttpStatusCode.OK && Last() == start.AddMinutes(4), "prefetch does not renew");
            }
            for (int i = 1; i <= 180; i++)
            {
                clock = start.AddMinutes(4 + i * 9);
                using (var response = await client.GetAsync("/open")) if (response.StatusCode != HttpStatusCode.OK) throw new InvalidOperationException("CHECK: sliding session expired");
            }
            Check(Last() == clock.Value, "27 simulated hours of HTTP activity without absolute lifetime cap");
            clock = clock.Value.AddMinutes(10);
            using (var response = await client.GetAsync("/auth")) Check(response.StatusCode == HttpStatusCode.Unauthorized && Count("SELECT COUNT(*) FROM ConnectedUsers") == 0, "exact IdleTime boundary revokes session");
        }
        clock = null;
    }

    static async Task HttpChecks()
    {
        using (var rsa = RSA.Create(2048))
        {
            var request = new CertificateRequest("CN=localhost", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            using (var ephemeral = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(1)))
            using (var certificate = new X509Certificate2(ephemeral.Export(X509ContentType.Pfx)))
            using (var host = new WebHostBuilder().UseKestrel(k => k.Listen(IPAddress.Loopback, 0, listener => listener.UseHttps(certificate)))
                .ConfigureServices(s => s.AddLogging(l => l.AddProvider(new AuditLogger())))
                .Configure(app => app.Run(Handle)).Build())
            {
                host.Start(); origin = new Uri(host.ServerFeatures.Get<IServerAddressesFeature>().Addresses.Single());
                await OrderedResponse("/auth", true);
                await OrderedResponse("/auth", expired: true);
                await OrderedResponse("/logout", true);
                await OrderedResponse("/logout");
                await OrderedResponse("/profile");
                await OrderedResponse("/initial", initial: true);
                await OrdinaryLogout(); await AuditFailure(); await JournalIdle();
            }
        }
    }

    static void ExecuteScript(string script)
    {
        var buffer = new StringBuilder(); string delimiter = ";";
        foreach (string line in script.Split('\n'))
        {
            if (line.TrimStart().StartsWith("DELIMITER ")) { delimiter = line.Trim().Substring(10); continue; }
            buffer.AppendLine(line);
            if (!line.TrimEnd().EndsWith(delimiter)) continue;
            string sql = buffer.ToString().TrimEnd(); Sql(sql.Substring(0, sql.Length - delimiter.Length)); buffer.Clear();
        }
    }
    static string Snapshot(string table)
    {
        using (var command = new MySqlCommand("SHOW CREATE TABLE `" + table + "`", connection))
        using (var reader = command.ExecuteReader()) { reader.Read(); return reader.GetString(1); }
    }
    static void Migration(string repo, string variant)
    {
        string prefix = "r22_" + Guid.NewGuid().ToString("N").Substring(0, 10) + "_";
        string users = prefix + "Users", sessions = prefix + "ConnectedUsers", marker = prefix + "SecurityInstallation", proc = prefix + "UpgradeSessionsV2";
        foreach (string name in new[] { users, sessions, marker }) Check(Count("SELECT COUNT(*) FROM information_schema.tables WHERE table_schema='test' AND table_name='" + name + "'") == 0, "owned fixture name absent");
        Check(Count("SELECT COUNT(*) FROM information_schema.routines WHERE routine_schema='test' AND routine_name='" + proc + "'") == 0, "owned procedure name absent");
        try
        {
            Sql("CREATE TABLE `" + users + "`(ID int NOT NULL AUTO_INCREMENT PRIMARY KEY,Password longtext NOT NULL) ENGINE=InnoDB");
            Sql("CREATE TABLE `" + marker + "`(ID int NOT NULL PRIMARY KEY,SchemaVersion int NOT NULL,LegacyPasswordResetCompleted tinyint(1) NOT NULL) ENGINE=InnoDB");
            Sql("CREATE TABLE `" + sessions + "`(ID int NOT NULL AUTO_INCREMENT PRIMARY KEY,UserID int NOT NULL UNIQUE,UserName varchar(255) NOT NULL,LoginKey varchar(255) NOT NULL,IPAdress varchar(255) NOT NULL,UserAgent varchar(255) NOT NULL,LastActionTime datetime(6) NOT NULL,CONSTRAINT `" + prefix + "fk` FOREIGN KEY(UserID) REFERENCES `" + users + "`(ID),UNIQUE INDEX legacy_four(UserName,LoginKey,IPAdress,UserAgent)) ENGINE=InnoDB CHARACTER SET utf8 COLLATE utf8_general_ci");
            Sql("INSERT INTO `" + users + "` VALUES(1,REPEAT('x',84))"); Sql("INSERT INTO `" + marker + "` VALUES(1,1,1)");
            Sql("INSERT INTO `" + sessions + "` VALUES(1,1,'synthetic','synthetic','127.0.0.1','review','2026-10-17 00:00:00')");
            if (variant == "name") Sql("ALTER TABLE `" + sessions + "` ADD INDEX `IX_" + sessions + "_TokenHash`(IPAdress)");
            if (variant == "dropped-column") Sql("ALTER TABLE `" + sessions + "` ADD INDEX user_custom(UserName)");
            if (variant == "prefix") Sql("ALTER TABLE `" + sessions + "` ADD INDEX user_custom(IPAdress(100))");
            if (variant == "functional") Sql("ALTER TABLE `" + sessions + "` ADD INDEX user_custom ((LENGTH(UserName)))");
            if (variant == "composite-primary") Sql("ALTER TABLE `" + sessions + "` DROP PRIMARY KEY, ADD PRIMARY KEY(ID,UserID)");
            if (variant == "duplicate-legacy") Sql("ALTER TABLE `" + sessions + "` ADD UNIQUE INDEX user_custom(UserName,LoginKey,IPAdress,UserAgent)");
            if (variant == "check") Sql("ALTER TABLE `" + sessions + "` ADD CONSTRAINT `" + prefix + "check` CHECK(LENGTH(UserName)>0)");
            if (variant == "trigger") Sql("CREATE TRIGGER `" + prefix + "trigger` BEFORE DELETE ON `" + sessions + "` FOR EACH ROW SET @review_trigger=1");
            if (variant == "outgoing-fk") Sql("ALTER TABLE `" + sessions + "` ADD CONSTRAINT `" + prefix + "extra` FOREIGN KEY(ID) REFERENCES `" + users + "`(ID)");
            string before = Snapshot(sessions);
            // [Комментарий ИИ-агента]: Однократная замена только имён позволяет выполнять реальную процедуру проекта на собственных таблицах внутри test.
            string script = Regex.Replace(File.ReadAllText(Path.Combine(repo, "tools/sql/UpgradeSessionsV2.sql")), "StoreCMS_UpgradeSessionsV2|SecurityInstallation|ConnectedUsers|Users",
                m => m.Value == "StoreCMS_UpgradeSessionsV2" ? proc : m.Value == "SecurityInstallation" ? marker : m.Value == "ConnectedUsers" ? sessions : users);
            if (variant == "success")
            {
                ExecuteScript(script);
                Check(Count("SELECT COUNT(*) FROM `" + sessions + "`") == 0, "successful migration revokes legacy fixture session");
                Check(Count("SELECT COUNT(*) FROM information_schema.columns WHERE table_schema='test' AND table_name='" + sessions + "'") == 6, "successful migration creates six-column layout");
                Sql("INSERT INTO `" + sessions + "`(UserID,TokenHash,LastActionUtc,IPAdress,UserAgent) VALUES(1,REPEAT('a',64),UTC_TIMESTAMP(6),'127.0.0.1','review')");
                ExecuteScript(script);
                Check(Count("SELECT COUNT(*) FROM `" + sessions + "`") == 1, "repeat successful migration preserves new session");
            }
            else
            {
                int number = 0; try { ExecuteScript(script); } catch (MySqlException error) { number = error.Number; }
                Check(number == 1644, "preflight safely rejects before ALTER: " + variant);
                Check(Snapshot(sessions) == before, "all session indexes and schema preserved on refusal: " + variant);
                Check(Count("SELECT COUNT(*) FROM `" + sessions + "` WHERE ID=1 AND UserID=1 AND UserName='synthetic' AND LoginKey='synthetic' AND IPAdress='127.0.0.1' AND UserAgent='review' AND LastActionTime='2026-10-17 00:00:00'") == 1, "legacy row values preserved on refusal: " + variant);
                Check(Count("SELECT COUNT(*) FROM `" + sessions + "`") == 1, "legacy row count unchanged on refusal");
                if (variant == "name")
                {
                    // [Комментарий ИИ-агента]: Только здесь тест явно удаляет свой конфликтный индекс; сама миграция пользовательские индексы не удаляет.
                    Sql("DROP PROCEDURE `" + proc + "`");
                    Sql("ALTER TABLE `" + sessions + "` DROP INDEX `IX_" + sessions + "_TokenHash`");
                    ExecuteScript(script);
                    Check(Count("SELECT COUNT(*) FROM information_schema.columns WHERE table_schema='test' AND table_name='" + sessions + "'") == 6, "migration retry succeeds after explicit removal of owned fixture conflict");
                }
            }
            Check(Count("SELECT COUNT(*) FROM `" + users + "` WHERE ID=1 AND Password=REPEAT('x',84)") == 1 && Count("SELECT COUNT(*) FROM `" + marker + "` WHERE ID=1 AND SchemaVersion=1 AND LegacyPasswordResetCompleted=1") == 1, "users and installation marker unchanged");
        }
        finally
        {
            Sql("DROP PROCEDURE IF EXISTS `" + proc + "`");
            foreach (string name in new[] { sessions, marker, users })
            { if (!name.StartsWith(prefix)) throw new InvalidOperationException("cleanup guard"); Sql("DROP TABLE IF EXISTS `" + name + "`"); }
        }
    }

    static async Task<int> Main(string[] args)
    {
        try
        {
            string repo = Path.GetFullPath(args[0]);
            string secret = Regex.Match(File.ReadAllText(Path.Combine(repo, "AGENTS.md")), @"(?m)^\| Пароль \| `([^`]+)` \|\r?$").Groups[1].Value;
            if (secret.Length == 0) throw new InvalidOperationException("Local credentials missing in AGENTS.md");
            using (connection = new MySqlConnection(new MySqlConnectionStringBuilder { Server = "127.0.0.1", Port = 3306, Database = "test", UserID = "root", Password = secret, SslMode = MySqlSslMode.Required, AllowUserVariables = true, Pooling = false }.ConnectionString))
            {
                connection.Open(); Console.WriteLine("Connected to existing local test; MySQL " + connection.ServerVersion);
                Check(Scalar("SELECT DATABASE()") == "test", "connection verified before fixture changes");
                string inventory = Scalar("SELECT GROUP_CONCAT(table_name ORDER BY table_name SEPARATOR '|') FROM information_schema.tables WHERE table_schema='test'");
                options = new DbContextOptionsBuilder<CMSDatabase>().UseMySql(connection).Options;
                await HttpChecks();
                Sql("DROP TEMPORARY TABLE AdminPanelLogs,ConnectedUsers,Users,UserTypes");
                foreach (string variant in new[] { "name", "dropped-column", "prefix", "functional", "composite-primary", "duplicate-legacy", "check", "trigger", "outgoing-fk", "success" }) Migration(repo, variant);
                Check(Scalar("SELECT GROUP_CONCAT(table_name ORDER BY table_name SEPARATOR '|') FROM information_schema.tables WHERE table_schema='test'") == inventory, "existing permanent table inventory unchanged after fixture cleanup");
            }
            Console.WriteLine("REGRESSION CHECKS PASSED: " + checks); return 0;
        }
        catch (Exception error)
        {
            Console.Error.WriteLine("FAILED: " + error.GetType().Name);
            if (error is HttpRequestException) Console.Error.WriteLine(error.ToString());
            if (error is MySqlException mysql) Console.Error.WriteLine("MySQL code: " + mysql.Number);
            if (error is InvalidOperationException && error.Message.StartsWith("CHECK:")) Console.Error.WriteLine(error.Message);
            return 1;
        }
    }
}
