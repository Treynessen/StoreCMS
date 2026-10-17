using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using Treynessen.AdminPanelTypes;
using Treynessen.Controllers;
using Treynessen.Database;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;
using Treynessen.Security;
using Treynessen.SettingsManagement;

internal static partial class SecurityIntegration
{
    private static string credentialPath, sourceRoot, serverConnection;
    private static readonly List<string> ownedDatabases = new List<string>();
    private static readonly List<string> ownedUsers = new List<string>();
    private static readonly ILoggerFactory commandCounter = new LoggerFactory(new[] { new CreationCounterProvider() });
    private static int creationCommands;
    private static int checks;
    internal static string HttpConnection, HttpRoot;
    private const string NewPassword = "Security-test-password-2026!";
    private static DbContextOptions<CMSDatabase> Options(string connection) => new DbContextOptionsBuilder<CMSDatabase>().UseMySql(connection).UseLoggerFactory(commandCounter).Options;
    private static void Check(bool ok, string name)
    { if (!ok) throw new InvalidOperationException("CHECK: " + name); checks++; Console.WriteLine("PASS: " + name); }

    public static int Main(string[] args)
    {
        try
        {
            int credentialIndex = Array.IndexOf(args, "--credential-config");
            int sourceIndex = Array.IndexOf(args, "--source-root");
            if (credentialIndex < 0) throw new InvalidOperationException("Private local MySQL credential file required");
            credentialPath = Path.GetFullPath(args[credentialIndex + 1]);
            var config = new ConfigurationBuilder().AddJsonFile(credentialPath).Build();
            var supplied = new MySqlConnectionStringBuilder(config["DbSettings:ConnectionString"]);
            if (supplied.UserID != "root") throw new InvalidOperationException("Local test credential identity must be root");
            // [Комментарий ИИ-агента]: Реквизиты используются только в памяти. Хост, порт и база из файла не переносятся в тестовое подключение.
            serverConnection = new MySqlConnectionStringBuilder { Server = "127.0.0.1", Port = supplied.Port, UserID = "root", Password = supplied.Password, ConnectionTimeout = 5, Pooling = false }.ConnectionString;
            if (args.Contains("--initialize-worker"))
            {
                string database = args[Array.IndexOf(args, "--database") + 1];
                if (!Regex.IsMatch(database, "\\Astorecms_security_test_[a-f0-9]{32}\\z")) throw new InvalidOperationException("Test database guard");
                using (var db = new CMSDatabase(Options(ForDatabase(database)))) MySqlSecurityInstallation.Initialize(db);
                Console.WriteLine("Worker initialized isolated database"); return 0;
            }
            if (args.Contains("--session-worker"))
            {
                string database = args[Array.IndexOf(args, "--database") + 1];
                if (!Regex.IsMatch(database, "\\Astorecms_security_test_[a-f0-9]{32}\\z")) throw new InvalidOperationException("Test database guard");
                using (var db = new CMSDatabase(Options(ForDatabase(database))))
                    return DatabaseInteraction.AddConnectedUser(db, db.Users.Single(), SessionContext()) ? 0 : 1;
            }
            if (sourceIndex < 0) throw new InvalidOperationException("Source root required for views and SQL script");
            sourceRoot = Path.GetFullPath(args[sourceIndex + 1]);
            using (var connection = new MySqlConnection(serverConnection))
            { connection.Open(); Console.WriteLine("Local isolated MySQL connected; version=" + connection.ServerVersion); }
            if (args.Contains("--sessions-only")) SessionChecks().GetAwaiter().GetResult();
            else Run().GetAwaiter().GetResult();
            Console.WriteLine("Completed integration checks: " + checks); return 0;
        }
        catch (Exception error)
        {
            Console.Error.WriteLine("FAILED: " + error.GetType().Name);
            if (error is InvalidOperationException && error.Message.StartsWith("CHECK: ")) Console.Error.WriteLine(error.Message);
            if (error is MySqlException mysql) Console.Error.WriteLine("MySQL number: " + mysql.Number);
            Console.Error.WriteLine("Completed checks: " + checks); return 1;
        }
        finally
        {
            // [Комментарий ИИ-агента]: Удаляются только собственные тестовые учётные записи и уникальные базы, отсутствовавшие до этого прогона.
            foreach (string user in ownedUsers)
            {
                try { Execute(serverConnection, "DROP USER IF EXISTS '" + user + "'@'127.0.0.1'"); Console.WriteLine("Removed owned isolated MySQL account"); }
                catch { Console.Error.WriteLine("Cleanup incomplete for owned test account"); }
            }
            foreach (string name in ownedDatabases)
            {
                try { Execute(serverConnection, "DROP DATABASE IF EXISTS `" + name + "`"); Console.WriteLine("Removed owned isolated database"); }
                catch { Console.Error.WriteLine("Cleanup incomplete for owned test database: " + name); }
            }
        }
    }

    private static string ForDatabase(string name)
    { var builder = new MySqlConnectionStringBuilder(serverConnection); builder.Database = name; return builder.ConnectionString; }
    private static string CreateDatabase()
    {
        string name = ReserveDatabase();
        Execute(serverConnection, "CREATE DATABASE `" + name + "` CHARACTER SET utf8 COLLATE utf8_general_ci");
        return name;
    }
    private static string ReserveDatabase()
    {
        string name = "storecms_security_test_" + Guid.NewGuid().ToString("N");
        if (Count(serverConnection, "SELECT COUNT(*) FROM information_schema.schemata WHERE schema_name='" + name + "'") != 0)
            throw new InvalidOperationException("Test database name collision");
        ownedDatabases.Add(name);
        return name;
    }
    private static void Execute(string connection, string sql)
    { using (var c = new MySqlConnection(connection)) { c.Open(); using (var command = new MySqlCommand(sql, c)) command.ExecuteNonQuery(); } }
    private static long Count(string connection, string sql)
    { using (var c = new MySqlConnection(connection)) { c.Open(); using (var command = new MySqlCommand(sql, c)) return Convert.ToInt64(command.ExecuteScalar()); } }
    private static bool Refuses(Action action)
    { try { action(); return false; } catch (InvalidOperationException) { return true; } }

    private static async Task Run()
    {
        await MissingDatabaseChecks();
        string name = CreateDatabase(), connection = ForDatabase(name);
        Check(Count(connection, "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema=DATABASE()") == 0, "new test database is empty");
        var workers = Enumerable.Range(0, 4).Select(_ => Task.Run(() =>
        {
            var info = new ProcessStartInfo("dotnet")
            {
                Arguments = "\"" + Assembly.GetExecutingAssembly().Location + "\" --initialize-worker --credential-config \"" + credentialPath + "\" --database " + name,
                UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true
            };
            using (var process = Process.Start(info))
            {
                string output = process.StandardOutput.ReadToEnd();
                string errors = process.StandardError.ReadToEnd();
                process.WaitForExit(); return process.ExitCode == 0;
            }
        })).ToArray();
        Check((await Task.WhenAll(workers)).All(x => x), "four parallel initializer processes succeed");
        string originalHash; int initialId;
        using (var db = new CMSDatabase(Options(connection)))
        {
            var user = db.Users.Include(u => u.UserType).Single(); initialId = user.ID; originalHash = user.Password;
            Check(user.Login == "admin" && SecurityFunctions.VerifyPassword(user, "admin") && user.RequirePasswordChange, "new installation creates modern admin/admin requiring change");
            Check(db.UserTypes.Count() == 1 && user.UserType.AccessLevel == AccessLevel.VeryHigh && db.SecurityInstallation.Count() == 1, "one role, one admin and one installation marker");
            Check(db.SecurityInstallation.Single().LegacyPasswordResetCompleted, "new installation disables legacy reset");
        }
        using (var db = new CMSDatabase(Options(connection))) MySqlSecurityInstallation.Initialize(db);
        using (var db = new CMSDatabase(Options(connection)))
            Check(db.Users.Single().Password == originalHash && db.Users.Single().ID == initialId, "restart preserves initial account and password hash");

        await HttpChecks(connection, initialId);
        using (var db = new CMSDatabase(Options(connection)))
        {
            var user = db.Users.Single();
            Check(!user.RequirePasswordChange && !SecurityFunctions.VerifyPassword(user, "admin") && SecurityFunctions.VerifyPassword(user, NewPassword), "stored new password works and admin no longer does");
        }
        using (var db = new CMSDatabase(Options(connection))) MySqlSecurityInstallation.Initialize(db);
        using (var db = new CMSDatabase(Options(connection)))
            Check(SecurityFunctions.VerifyPassword(db.Users.Single(), NewPassword), "restart preserves changed password");

        // [Комментарий ИИ-агента]: Два контекста с устаревшими данными не должны оба успешно менять первоначальный пароль.
        using (var db = new CMSDatabase(Options(connection)))
        { var user = db.Users.Single(); user.Password = SecurityFunctions.GetPasswordHash("admin"); user.RequirePasswordChange = true; db.SaveChanges(); }
        using (var first = new CMSDatabase(Options(connection)))
        using (var second = new CMSDatabase(Options(connection)))
        {
            var one = first.Users.Single(); var two = second.Users.Single();
            var model = new UserModel { CurrentPassword = "admin", NewPassword = NewPassword, ConfirmPassword = NewPassword };
            DatabaseInteraction.ChangeInitialPassword(first, one, model, out int status1);
            model.NewPassword = model.ConfirmPassword = "Different-test-password-2026!";
            DatabaseInteraction.ChangeInitialPassword(second, two, model, out int status2);
            Check(status1 == 200 && status2 == 409, "stale parallel password change cannot overwrite the winner");
        }
        using (var db = new CMSDatabase(Options(connection)))
        { var user = db.Users.Include(u => u.UserType).Single(); user.UserType.AccessLevel = AccessLevel.Low; db.SaveChanges(); }
        using (var db = new CMSDatabase(Options(connection))) MySqlSecurityInstallation.Initialize(db);
        using (var db = new CMSDatabase(Options(connection)))
            Check(db.Users.Count() == 1 && db.UserTypes.Count(t => t.AccessLevel == AccessLevel.VeryHigh) == 0, "loss of admin rights does not bootstrap a new role or account");
        using (var db = new CMSDatabase(Options(connection))) { db.Users.Remove(db.Users.Single()); db.SaveChanges(); }
        await Task.WhenAll(Enumerable.Range(0, 3).Select(_ => Task.Run(() => { using (var db = new CMSDatabase(Options(connection))) MySqlSecurityInstallation.Initialize(db); })));
        using (var db = new CMSDatabase(Options(connection))) Check(db.Users.Count() == 0, "parallel restarts after admin deletion never restore admin/admin");

        LegacyChecks();
        StorageCompatibilityChecks();
        string partial = ForDatabase(CreateDatabase());
        using (var db = new CMSDatabase(Options(partial))) db.Database.EnsureCreated();
        using (var db = new CMSDatabase(Options(partial)))
            Check(Refuses(() => MySqlSecurityInstallation.Initialize(db)), "schema without committed marker fails closed");
        Execute(partial, "INSERT INTO SecurityInstallation VALUES (1,999,UTC_TIMESTAMP(6),1)");
        using (var db = new CMSDatabase(Options(partial)))
            Check(Refuses(() => MySqlSecurityInstallation.Initialize(db)), "unsupported schema version fails closed");
    }

    private static async Task MissingDatabaseChecks()
    {
        string previousName = ReserveDatabase();
        using (var db = new CMSDatabase(Options(ForDatabase(previousName))))
            db.GetService<IRelationalDatabaseCreator>().Create();
        Check(Count(serverConnection, "SELECT COUNT(*) FROM information_schema.schemata WHERE schema_name='" + previousName + "'") == 1,
            "previous EF database creator supports automatic creation of an absent database");
        string name = ReserveDatabase(), connection = ForDatabase(name);
        Check(Count(serverConnection, "SELECT COUNT(*) FROM information_schema.schemata WHERE schema_name='" + name + "'") == 0, "missing database fixture does not exist");
        var workers = Enumerable.Range(0, 2).Select(_ => Task.Run(() =>
        {
            var info = new ProcessStartInfo("dotnet")
            {
                Arguments = "\"" + Assembly.GetExecutingAssembly().Location + "\" --initialize-worker --credential-config \"" + credentialPath + "\" --database " + name,
                UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true
            };
            using (var process = Process.Start(info))
            {
                process.StandardOutput.ReadToEnd(); process.StandardError.ReadToEnd(); process.WaitForExit();
                return process.ExitCode == 0;
            }
        })).ToArray();
        Check((await Task.WhenAll(workers)).All(ok => ok), "two application initialization processes succeed when database is absent");
        using (var db = new CMSDatabase(Options(connection)))
        {
            var user = db.Users.Single();
            Check(user.Login == "admin" && user.RequirePasswordChange && SecurityFunctions.VerifyPassword(user, "admin"), "absent database is created and initialized automatically");
            Check(db.SecurityInstallation.Count() == 1 && db.UserTypes.Count() == 1, "concurrent starts on absent database create one installation");
            string hash = user.Password;
            MySqlSecurityInstallation.Initialize(db);
            Check(db.Users.Single().Password == hash && db.Users.Count() == 1, "restart of automatically created database preserves account and hash");
            Check(db.Database.GetDbConnection().State == System.Data.ConnectionState.Closed, "initialization closes its target connection");
        }

        string userName = "sc_" + Guid.NewGuid().ToString("N").Substring(0, 20);
        string password = Guid.NewGuid().ToString("N") + "!";
        using (var c = new MySqlConnection(serverConnection))
        {
            c.Open();
            using (var command = new MySqlCommand("CREATE USER '" + userName + "'@'127.0.0.1' IDENTIFIED BY @password", c))
            { command.Parameters.AddWithValue("@password", password); command.ExecuteNonQuery(); }
        }
        ownedUsers.Add(userName);
        string deniedName = ReserveDatabase();
        Execute(serverConnection, "GRANT SELECT ON `" + deniedName + "`.* TO '" + userName + "'@'127.0.0.1'");
        var restricted = new MySqlConnectionStringBuilder(serverConnection) { Database = deniedName, UserID = userName, Password = password };
        int before = creationCommands;
        using (var db = new CMSDatabase(Options(restricted.ConnectionString)))
        {
            bool refused = false;
            try { MySqlSecurityInstallation.Initialize(db); }
            catch (InvalidOperationException error)
            { refused = error.Message.Contains("CREATE DATABASE") && error.InnerException is MySqlException; }
            Check(refused && creationCommands > before, "missing CREATE DATABASE privilege produces a clear error");
        }
        Check(Count(serverConnection, "SELECT COUNT(*) FROM information_schema.schemata WHERE schema_name='" + deniedName + "'") == 0, "denied CREATE DATABASE leaves database absent");
        Check(Count(serverConnection, "SELECT IS_FREE_LOCK(CONCAT('storecms-security:',LEFT(SHA2('" + deniedName + "',256),40)))") == 1, "failure to create releases installation lock");

        var incorrect = new MySqlConnectionStringBuilder(restricted.ConnectionString) { Password = password + "wrong" };
        before = creationCommands;
        using (var db = new CMSDatabase(Options(incorrect.ConnectionString)))
        {
            bool refused = false;
            try { MySqlSecurityInstallation.Initialize(db); }
            catch (MySqlException error) { refused = error.Number == 1045; }
            Check(refused && creationCommands == before, "incorrect MySQL password never attempts CREATE DATABASE");
        }
        var inaccessible = new MySqlConnectionStringBuilder(restricted.ConnectionString) { Database = name };
        before = creationCommands;
        using (var db = new CMSDatabase(Options(inaccessible.ConnectionString)))
        {
            bool refused = false;
            try { MySqlSecurityInstallation.Initialize(db); }
            catch (MySqlException error) { refused = error.Number == 1044; }
            Check(refused && creationCommands == before, "access denied to existing database never attempts CREATE DATABASE");
        }
        var unavailable = new MySqlConnectionStringBuilder(serverConnection) { Database = ReserveDatabase(), Port = 1, ConnectionTimeout = 1 };
        before = creationCommands;
        using (var db = new CMSDatabase(Options(unavailable.ConnectionString)))
        {
            bool refused = false;
            try { MySqlSecurityInstallation.Initialize(db); }
            catch (MySqlException) { refused = true; }
            Check(refused && creationCommands == before, "unavailable MySQL server never attempts CREATE DATABASE");
        }
        using (var db = new CMSDatabase(Options(connection)))
            Check(db.Users.Count() == 1 && db.SecurityInstallation.Count() == 1, "negative connection scenarios preserve existing installation");
    }

    private sealed class CreationCounterProvider : ILoggerProvider, ILogger
    {
        public ILogger CreateLogger(string categoryName) => this;
        public bool IsEnabled(LogLevel logLevel) => true;
        public IDisposable BeginScope<TState>(TState state) => null;
        public void Dispose() { }
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            // [Комментарий ИИ-агента]: Считаются только попытки CREATE DATABASE; SQL, реквизиты и сообщения провайдера не выводятся и не сохраняются.
            if (formatter(state, exception).IndexOf("CREATE DATABASE", StringComparison.OrdinalIgnoreCase) >= 0)
                Interlocked.Increment(ref creationCommands);
        }
    }

    private static async Task HttpChecks(string connection, int initialId)
    {
        HttpConnection = connection;
        HttpRoot = Path.Combine(sourceRoot, "StoreCMS");
        var hosts = await Task.WhenAll(Task.Run(() => StartHost()), Task.Run(() => StartHost()));
        using (var firstHost = hosts[0])
        using (var secondHost = hosts[1])
        {
            Check(firstHost != null && secondHost != null, "two HTTP application hosts start in parallel on initialized database");
            string address = firstHost.ServerFeatures.Get<IServerAddressesFeature>().Addresses.Single();
            var handler = new HttpClientHandler { AllowAutoRedirect = false, CookieContainer = new CookieContainer(), ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true };
            using (var client = new HttpClient(handler) { BaseAddress = new Uri(address) })
            {
                client.DefaultRequestHeaders.UserAgent.ParseAdd("StoreCMS-security-tests/1.0");
                const string path = "/private/control-room";
                var login = await Send(client, path, "POST", AdminPanelPages.LoginForm, "Login", "admin", "Password", "admin");
                Check(login.StatusCode == HttpStatusCode.OK && handler.CookieContainer.GetCookies(new Uri(new Uri(address), path)).Cast<Cookie>().Count(c => c.Name == SessionCookies.Name) == 1, "HTTPS admin/admin login issues one session cookie at custom panel URL");
                var page = await client.GetAsync(path + "?pageID=" + (int)AdminPanelPages.Settings);
                string body = await page.Content.ReadAsStringAsync();
                Check(page.StatusCode == HttpStatusCode.OK && body.Contains("UserModel.ConfirmPassword") && body.Contains("action=\"" + path + "\""), "GET settings returns only mandatory change form with current URL");
                Check(!body.Contains("id=\"admin-panel-menu\"") && !body.Contains("SettingsModel."), "restricted form contains no settings controls");
                Check((await Send(client, path, "POST", AdminPanelPages.AddUserType, "UserTypeModel.Name", "forbidden", "UserTypeModel.AccessLevel", "5")).StatusCode == HttpStatusCode.Forbidden, "direct POST operation is blocked before password change");
                using (var content = new MultipartFormDataContent())
                {
                    content.Add(new StringContent(((int)AdminPanelPages.UploadFile).ToString()), "pageID");
                    Check((await client.PutAsync(path, content)).StatusCode == HttpStatusCode.Forbidden, "direct PUT upload is blocked before password change");
                }
                Check((await Send(client, path, "DELETE", AdminPanelPages.DeleteUser, "itemID", initialId.ToString())).StatusCode == HttpStatusCode.Forbidden, "direct DELETE is blocked before password change");
                Check((await Send(client, path, "DELETE", AdminPanelPages.Exit)).StatusCode == HttpStatusCode.OK, "exit remains available while password change required");
                Check((await Send(client, path, "POST", AdminPanelPages.LoginForm, "Login", "admin", "Password", "admin")).StatusCode == HttpStatusCode.OK, "initial login still works before completed change");
                Check((await Change(client, path, "wrong", NewPassword, NewPassword)).StatusCode == HttpStatusCode.Forbidden, "wrong current password rejected on server");
                Check((await Change(client, path, "admin", "admin", "admin")).StatusCode == HttpStatusCode.UnprocessableEntity, "default password cannot be chosen again");
                Check((await Change(client, path, "admin", NewPassword, "Mismatch")).StatusCode == HttpStatusCode.UnprocessableEntity, "password confirmation checked on server");
                var changed = await Change(client, path, "admin", NewPassword, NewPassword);
                Check(changed.StatusCode == HttpStatusCode.Redirect && changed.Headers.Location.ToString() == path, "valid change redirects to current custom panel URL");
                Check((await Send(client, path, "POST", AdminPanelPages.LoginForm, "Login", "admin", "Password", NewPassword)).StatusCode == HttpStatusCode.OK, "new password login required after initial change");
                var home = await client.GetAsync(path);
                string html = await home.Content.ReadAsStringAsync();
                Check(home.StatusCode == HttpStatusCode.OK && html.Contains("admin") && !html.Contains("UserModel.ConfirmPassword"), "real administrative Razor home works after password change");
                Check((await client.GetAsync(path + "?pageID=" + (int)AdminPanelPages.Pages)).StatusCode == HttpStatusCode.OK, "real administrative page list renders after change");
                Check((await Send(client, path, "POST", AdminPanelPages.AddUserType, "UserTypeModel.Name", "security-test-role", "UserTypeModel.AccessLevel", "1")).StatusCode == HttpStatusCode.Created, "ordinary administrative state-changing operation works after change");
                Check((await Change(client, path, NewPassword, "Another-test-password!", "Another-test-password!")).StatusCode == HttpStatusCode.Forbidden, "initial-change operation refuses after completion");
                Check((await Send(client, path, "DELETE", AdminPanelPages.Exit)).StatusCode == HttpStatusCode.OK, "ordinary exit works");
                Check((await Send(client, path, "POST", AdminPanelPages.LoginForm, "Login", "admin", "Password", "admin")).StatusCode == HttpStatusCode.Unauthorized, "HTTP admin/admin fails after change");
                Check((await Send(client, path, "POST", AdminPanelPages.LoginForm, "Login", "admin", "Password", "wrong")).StatusCode == HttpStatusCode.Unauthorized, "HTTP incorrect new password fails");
                Check((await Send(client, path, "POST", AdminPanelPages.LoginForm, "Login", "admin", "Password", NewPassword)).StatusCode == HttpStatusCode.OK, "HTTP new password authenticates");
                Check((await client.GetAsync(path)).StatusCode == HttpStatusCode.OK, "panel accessible on subsequent new-password login");
                Check((await Send(client, path, "POST", AdminPanelPages.EditUserData, "UserModel.ID", initialId.ToString(), "UserModel.Login", "admin", "UserModel.IdleTime", "10", "UserModel.CurrentPassword", "wrong")).StatusCode == HttpStatusCode.Forbidden, "profile rejects incorrect current password with modern hashing");
                Check((await Send(client, path, "POST", AdminPanelPages.EditUserData, "UserModel.ID", initialId.ToString(), "UserModel.Login", "admin", "UserModel.IdleTime", "10", "UserModel.CurrentPassword", NewPassword)).StatusCode == HttpStatusCode.OK, "profile verifies modern current password and remains usable");
                Check((await Send(client, path, "POST", AdminPanelPages.EditUserData, "UserModel.ID", initialId.ToString(), "UserModel.Login", "admin", "UserModel.IdleTime", "10", "UserModel.CurrentPassword", NewPassword, "UserModel.NewPassword", "admin")).StatusCode == HttpStatusCode.UnprocessableEntity, "profile cannot restore the default password");
            }
        }
    }
    private static Task<HttpResponseMessage> Change(HttpClient client, string path, string current, string next, string confirm) =>
        Send(client, path, "POST", AdminPanelPages.ChangeInitialPassword, "UserModel.CurrentPassword", current, "UserModel.NewPassword", next, "UserModel.ConfirmPassword", confirm);
    private static Task<HttpResponseMessage> Send(HttpClient client, string path, string method, AdminPanelPages page, params string[] fields)
    {
        var values = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("pageID", ((int)page).ToString()) };
        for (int i = 0; i < fields.Length; i += 2) values.Add(new KeyValuePair<string, string>(fields[i], fields[i + 1]));
        return client.SendAsync(new HttpRequestMessage(new HttpMethod(method), path) { Content = new FormUrlEncodedContent(values) });
    }
    internal static System.Security.Cryptography.X509Certificates.X509Certificate2 TestCertificate()
    {
        using (var rsa = System.Security.Cryptography.RSA.Create(2048))
        {
            var request = new System.Security.Cryptography.X509Certificates.CertificateRequest("CN=localhost", rsa, System.Security.Cryptography.HashAlgorithmName.SHA256, System.Security.Cryptography.RSASignaturePadding.Pkcs1);
            using (var certificate = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(1)))
                return new System.Security.Cryptography.X509Certificates.X509Certificate2(certificate.Export(System.Security.Cryptography.X509Certificates.X509ContentType.Pfx));
        }
    }

    private static IWebHost StartHost()
    {
        var host = new WebHostBuilder().UseKestrel(options => options.Listen(IPAddress.Loopback, 0, listen => listen.UseHttps(TestCertificate()))).UseContentRoot(HttpRoot)
            .UseSetting(WebHostDefaults.ApplicationKey, typeof(AdminPanelController).Assembly.GetName().Name)
            .UseStartup<SecurityTestStartup>().Build();
        using (var scope = host.Services.CreateScope()) MySqlSecurityInstallation.Initialize(scope.ServiceProvider.GetRequiredService<CMSDatabase>());
        host.Start(); return host;
    }

    private static void LegacyChecks()
    {
        string connection = ForDatabase(CreateDatabase());
        using (var db = new CMSDatabase(Options(connection)))
        {
            db.Database.EnsureCreated();
            var user = new User { ID = 42, Login = "existing-owner", Password = new string('a', 32), IdleTime = 20, Email = "test@example.invalid", UserType = new UserType { Name = "Owner", AccessLevel = AccessLevel.VeryHigh } };
            db.Users.Add(user);
            db.AdminPanelLogs.Add(new AdminPanelLog { User = user, Info = "Existing linked data", Time = DateTime.UtcNow });
            db.ConnectedUsers.Add(new ConnectedUser { User = user, TokenHash = new string('a', 64), IPAdress = "127.0.0.1", UserAgent = "integration", LastActionUtc = DateTime.UtcNow });
            db.SaveChanges();
        }
        Execute(connection, "DROP TABLE SecurityInstallation");
        Execute(connection, "ALTER TABLE Users DROP COLUMN RequirePasswordChange");
        using (var db = new CMSDatabase(Options(connection)))
            Check(Refuses(() => MySqlSecurityInstallation.Initialize(db)), "old schema is not auto-upgraded or bootstrapped");
        Check(Count(connection, "SELECT COUNT(*) FROM Users WHERE ID=42") == 1, "refused startup leaves legacy ID intact");
        ApplySecurityUpgrade(connection);
        using (var db = new CMSDatabase(Options(connection)))
        {
            using (MySqlSecurityInstallation.AcquireLock(db)) MySqlSecurityInstallation.Validate(db);
            Check(db.Users.Single().ID == 42 && !db.SecurityInstallation.Single().LegacyPasswordResetCompleted, "explicit SQL upgrades legacy schema without replacing user");
            Check(Refuses(() => MySqlSecurityInstallation.Initialize(db)), "upgraded site cannot start before one-time password replacement");
            Check(Refuses(() => LegacyPasswordReplacement.Replace(db, 41, NewPassword)), "replacement refuses wrong administrator ID");
        }
        using (var db = new CMSDatabase(Options(connection)))
            Check(Refuses(() => LegacyPasswordReplacement.Replace(db, 42, "admin")), "replacement refuses known default password");
        var attempts = Enumerable.Range(0, 2).Select(_ => Task.Run(() =>
        {
            using (var db = new CMSDatabase(Options(connection)))
                return !Refuses(() => LegacyPasswordReplacement.Replace(db, 42, NewPassword));
        })).ToArray();
        Task.WaitAll(attempts);
        Check(attempts.Count(t => t.Result) == 1, "exactly one parallel legacy replacement succeeds");
        using (var db = new CMSDatabase(Options(connection)))
        {
            var user = db.Users.Single();
            Check(user.ID == 42 && user.Login == "existing-owner" && user.Email == "test@example.invalid" && user.IdleTime == 20 && user.UserTypeID != 0, "replacement preserves ID, login, profile and role link");
            Check(db.AdminPanelLogs.Single().UserId == 42 && db.AdminPanelLogs.Single().Info == "Existing linked data", "existing linked log retained");
            Check(db.ConnectedUsers.Count() == 0 && db.SecurityInstallation.Single().LegacyPasswordResetCompleted, "replacement removes prior session and commits one-time marker");
            Check(SecurityFunctions.VerifyPassword(user, NewPassword) && !SecurityFunctions.VerifyPassword(user, "wrong"), "replacement password verifies, incorrect password fails");
            Check(Refuses(() => LegacyPasswordReplacement.Replace(db, 42, "Another-test-password!")), "completed legacy replacement cannot be repeated");
        }
        using (var db = new CMSDatabase(Options(connection))) MySqlSecurityInstallation.Initialize(db);
        Check(Count(connection, "SELECT COUNT(*) FROM Users") == 1 && Count(connection, "SELECT COUNT(*) FROM Users WHERE Login='admin'") == 0, "upgraded legacy database never receives admin/admin");
    }

    private static void ApplySecurityUpgrade(string connection, string scriptPath = "tools/sql/UpgradeSecurityV1.sql")
    {
        string script = File.ReadAllText(Path.Combine(sourceRoot, scriptPath));
        using (var c = new MySqlConnection(new MySqlConnectionStringBuilder(connection) { AllowUserVariables = true }.ConnectionString))
        {
            c.Open(); var buffer = new StringBuilder(); string delimiter = ";";
            foreach (string line in script.Split('\n'))
            {
                if (line.TrimStart().StartsWith("DELIMITER ")) { delimiter = line.Trim().Substring(10); continue; }
                buffer.AppendLine(line);
                if (!line.TrimEnd().EndsWith(delimiter)) continue;
                string sql = buffer.ToString().TrimEnd(); sql = sql.Substring(0, sql.Length - delimiter.Length);
                using (var command = new MySqlCommand(sql, c)) command.ExecuteNonQuery();
                buffer.Clear();
            }
        }
    }

    private static string StorageFixture()
    {
        string connection = ForDatabase(CreateDatabase());
        using (var db = new CMSDatabase(Options(connection)))
        {
            db.Database.EnsureCreated();
            var user = new User { ID = 42, Login = "storage-owner", Password = new string('a', 32), IdleTime = 20,
                UserType = new UserType { Name = "Owner", AccessLevel = AccessLevel.VeryHigh } };
            db.Users.Add(user);
            db.ConnectedUsers.Add(new ConnectedUser { User = user, TokenHash = new string('b', 64), IPAdress = "127.0.0.1", UserAgent = "fixture", LastActionUtc = DateTime.UtcNow });
            db.AdminPanelLogs.Add(new AdminPanelLog { User = user, Info = "Preserve storage fixture", Time = DateTime.UtcNow });
            db.SaveChanges();
        }
        Check(Count(connection, "SELECT COUNT(*) FROM information_schema.columns WHERE table_schema=DATABASE() AND table_name='Users' AND column_name='Password' AND data_type='longtext'") == 1,
            "actual EF/Pomelo Password column is longtext without a length limit in the model");
        Execute(connection, "DROP TABLE SecurityInstallation");
        Execute(connection, "ALTER TABLE Users DROP COLUMN RequirePasswordChange");
        return connection;
    }

    private static string StorageSnapshot(string connection)
    {
        // [Комментарий ИИ-агента]: Снимок всех полей синтетических записей остаётся в памяти и никогда не выводится.
        var snapshot = new StringBuilder();
        using (var c = new MySqlConnection(connection))
        {
            c.Open();
            foreach (string table in new[] { "Users", "UserTypes", "ConnectedUsers", "AdminPanelLogs", "SecurityInstallation" })
            {
                using (var exists = new MySqlCommand("SELECT COUNT(*) FROM information_schema.tables WHERE table_schema=DATABASE() AND table_name=@table", c))
                {
                    exists.Parameters.AddWithValue("@table", table);
                    if (Convert.ToInt64(exists.ExecuteScalar()) == 0) continue;
                }
                using (var command = new MySqlCommand("SELECT * FROM `" + table + "` ORDER BY 1", c))
                using (var reader = command.ExecuteReader())
                {
                    snapshot.Append(table);
                    for (int i = 0; i < reader.FieldCount; i++) snapshot.Append('|').Append(reader.GetName(i));
                    while (reader.Read()) for (int i = 0; i < reader.FieldCount; i++)
                    {
                        object value = reader.GetValue(i);
                        string text = value is byte[] bytes ? Convert.ToBase64String(bytes) : Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture);
                        snapshot.Append('|').Append(text.Length).Append(':').Append(text);
                    }
                }
            }
        }
        return snapshot.ToString();
    }

    private static void StorageCompatibilityChecks()
    {
        foreach (string type in new[] { "varchar(32)", "varchar(83)", "varbinary(84)", "varchar(84)", "char(84)", "varchar(85)", "text", "longtext" })
        {
            string connection = StorageFixture();
            Execute(connection, "ALTER TABLE Users MODIFY Password " + type + " NOT NULL");
            string before = StorageSnapshot(connection);
            bool compatible = type != "varchar(32)" && type != "varchar(83)" && type != "varbinary(84)";
            bool refused = false;
            try { ApplySecurityUpgrade(connection); }
            catch (MySqlException error) { refused = error.Number == 1644 && error.Message.Contains("Users.Password"); }
            Check(refused != compatible, "SQL Password compatibility: " + type);
            Check(Count(connection, "SELECT COUNT(*) FROM information_schema.columns WHERE table_schema=DATABASE() AND table_name='Users' AND column_name='Password' AND column_type='" + type + "'") == 1,
                "SQL never changes Password type or capacity: " + type);
            if (!compatible)
            {
                Check(StorageSnapshot(connection) == before, "SQL refusal preserves all fixture data: " + type);
                Check(Count(connection, "SELECT COUNT(*) FROM information_schema.columns WHERE table_schema=DATABASE() AND table_name='Users' AND column_name='RequirePasswordChange'") == 0
                    && Count(connection, "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema=DATABASE() AND table_name='SecurityInstallation'") == 0,
                    "incompatible Password refuses before security DDL: " + type);
                continue;
            }
            Check(Count(connection, "SELECT COUNT(*) FROM Users WHERE ID=42 AND Password='" + new string('a', 32) + "'") == 1, "SQL upgrade preserves legacy password: " + type);
            using (var db = new CMSDatabase(Options(connection))) LegacyPasswordReplacement.Replace(db, 42, NewPassword);
            using (var db = new CMSDatabase(Options(connection)))
                Check(db.Users.Single().Password.Length == 84 && SecurityFunctions.VerifyPassword(db.Users.Single(), NewPassword), "InnoDB replacement stores and verifies full hash: " + type);
        }

        string narrowed = StorageFixture();
        ApplySecurityUpgrade(narrowed);
        Execute(narrowed, "ALTER TABLE Users MODIFY Password varchar(83) NOT NULL");
        CheckStorageRefusal(narrowed, "Users.Password", "Password narrowed after SQL upgrade");

        foreach (string table in new[] { "Users", "ConnectedUsers", "SecurityInstallation" })
        {
            string connection = StorageFixture();
            ApplySecurityUpgrade(connection);
            // [Комментарий ИИ-агента]: Только в собственной фикстуре создаём MyISAM-копию без внешних ключей; исходные связи сохраняются на переименованной таблице.
            Execute(connection, "RENAME TABLE `" + table + "` TO `storage_original`");
            Execute(connection, "CREATE TABLE `" + table + "` LIKE storage_original");
            // [Комментарий ИИ-агента]: У MyISAM меньше предел длины ключа; только тестовая копия сессий не нуждается в составном индексе для проверки отказа.
            if (table == "ConnectedUsers")
                Execute(connection, "ALTER TABLE ConnectedUsers DROP INDEX IX_ConnectedUsers_TokenHash");
            Execute(connection, "ALTER TABLE `" + table + "` ENGINE=MyISAM");
            Execute(connection, "INSERT INTO `" + table + "` SELECT * FROM storage_original");
            Check(Count(connection, "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema=DATABASE() AND table_name='" + table + "' AND engine='MyISAM'") == 1, "actual MyISAM fixture: " + table);
            CheckStorageRefusal(connection, "InnoDB", "nontransactional " + table);
        }
    }

    private static void CheckStorageRefusal(string connection, string message, string scenario)
    {
        string before = StorageSnapshot(connection);
        bool refused = false;
        using (var db = new CMSDatabase(Options(connection)))
        {
            try { LegacyPasswordReplacement.Replace(db, 42, NewPassword); }
            catch (LegacyPasswordReplacement.StorageCompatibilityException error) { refused = error.Message.Contains(message); }
            Check(refused && !db.ChangeTracker.HasChanges(), "storage refusal precedes tracked changes: " + scenario);
        }
        Check(StorageSnapshot(connection) == before, "storage refusal preserves password, session, marker and linked data: " + scenario);
    }
}

internal class SecurityTestStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        new Startup().ConfigureServices(services);
        services.RemoveAll<IHostedService>(); // [Комментарий ИИ-агента]: Таймеры рабочего приложения не запускаются в интеграционном хосте.
        services.RemoveAll<CMSDatabase>(); services.RemoveAll<DbContextOptions<CMSDatabase>>();
        services.RemoveAll<ConfigurationHandler>();
        services.AddDbContext<CMSDatabase>(o => o.UseMySql(SecurityIntegration.HttpConnection));
        string config = Path.Combine(AppContext.BaseDirectory, "safe-test-settings-" + Guid.NewGuid().ToString("N") + ".json");
        File.WriteAllText(config, "{\"AdminPanelAccessSettings\":{}}"); // [Комментарий ИИ-агента]: Реквизиты и рабочая конфигурация не копируются.
        services.AddScoped(_ => new ConfigurationHandler(config));
        services.AddMvc().AddApplicationPart(typeof(AdminPanelController).Assembly);
    }
    public void Configure(IApplicationBuilder app)
    {
        app.Use(async (context, next) =>
        {
            try { await next(); }
            catch (Exception error) { Console.WriteLine("HTTP error type=" + error.GetType().Name); throw; }
        });
        app.UseMvc(routes => routes.MapRoute("custom_admin", "private/control-room", new { controller = "AdminPanel", action = "AdminPanel" }));
    }
}
