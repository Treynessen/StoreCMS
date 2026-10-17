using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.EntityFrameworkCore;
using Treynessen.AdminPanelTypes;
using Treynessen.Database;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;
using Treynessen.Security;

internal static partial class SecurityIntegration
{
    private static DefaultHttpContext SessionContext(string token = null, string method = "GET")
    {
        var context = new DefaultHttpContext();
        context.Request.Scheme = "https"; context.Request.Method = method;
        context.Connection.RemoteIpAddress = IPAddress.Loopback;
        if (token != null) context.Request.Headers["Cookie"] = SessionCookies.Name + "=" + token;
        return context;
    }

    private static string Issue(string connection)
    {
        using (var db = new CMSDatabase(Options(connection)))
        {
            var context = SessionContext();
            Check(DatabaseInteraction.AddConnectedUser(db, db.Users.Single(), context), "session issued after current password check");
            string header = context.Response.Headers["Set-Cookie"].Single(s => s.StartsWith(SessionCookies.Name + "="));
            Check(header.Contains("secure") && header.Contains("httponly") && header.Contains("samesite=lax") && header.Contains("path=/"), "serialized secure cookie attributes on runtime 2.2");
            Check(!header.Contains("expires=") && !header.Contains("max-age=") && !header.Contains("domain="), "host-only nonpersistent session cookie");
            return header.Split(';')[0].Substring(SessionCookies.Name.Length + 1);
        }
    }

    private static User Authenticate(string connection, string token, DateTime now, string method = "GET", bool prefetch = false)
    {
        using (var db = new CMSDatabase(Options(connection)))
        {
            var context = SessionContext(token, method);
            if (prefetch) context.Request.Headers["Sec-Purpose"] = "prefetch";
            return SessionStore.Authenticate(db, context, AdminSessionPolicy.Instance, () => now);
        }
    }

    private static void SessionUnitChecks()
    {
        var tokens = new HashSet<string>();
        for (int i = 0; i < 2000; i++) tokens.Add(SessionToken.Create());
        Check(tokens.Count == 2000 && tokens.All(t => t.Length == 43), "CSPRNG format and uniqueness smoke check");
        string token = tokens.First();
        Check(SessionToken.TryHash(token, out string hash) && hash.Length == 64, "SHA256 token digest");
        Check(SessionToken.TryHash(token, out string repeated) && hash == repeated, "stable hash for same token bytes");
        foreach (string bad in new[] { null, "", "abc", new string('a', 44), new string('!', 43), token + "=", hash, new string('A', 42) + "B" })
            Check(!SessionToken.TryHash(bad, out _), "invalid or noncanonical token rejected");
        var user = new User { IdleTime = 10 };
        var now = new DateTime(2026, 10, 16, 12, 0, 0, DateTimeKind.Utc);
        var session = new ConnectedUser { LastActionUtc = now };
        Check(!AdminSessionPolicy.Instance.IsExpired(session, user, now.AddMinutes(10).AddTicks(-1)), "idle just before boundary");
        Check(AdminSessionPolicy.Instance.IsExpired(session, user, now.AddMinutes(10)), "idle exact boundary");
        Check(AdminSessionPolicy.Instance.IsExpired(session, user, now.AddMinutes(11)), "idle past boundary");
        for (int i = 0; i < 200; i++)
        {
            now = now.AddMinutes(9);
            if (AdminSessionPolicy.Instance.IsExpired(session, user, now)) throw new InvalidOperationException("CHECK: active admin unexpectedly expired");
            session.LastActionUtc = now;
        }
        Check(true, "meaningful activity beyond 8 hours and 24 hours preserves admin session");
        Check(!AdminSessionPolicy.Instance.ShouldRenew(SessionContext(method: "HEAD")), "HEAD does not renew");
        Check(!AdminSessionPolicy.Instance.ShouldRenew(SessionContext(method: "OPTIONS")), "OPTIONS does not renew");
        var prefetch = SessionContext(); prefetch.Request.Headers["Purpose"] = "prefetch";
        Check(!AdminSessionPolicy.Instance.ShouldRenew(prefetch), "browser prefetch does not renew");
        var http = SessionContext(); http.Request.Scheme = "http";
        Check(Refuses(() => SessionCookies.Write(http, token)), "HTTP cannot issue session cookie");
        SessionCookies.ClearLegacy(prefetch);
        Check(prefetch.Response.Headers["Set-Cookie"].Count == 2 && prefetch.Response.Headers["Set-Cookie"].All(s => s.Contains("expires=")), "only legacy cookies deleted; current cookie is never expired by a late response");
    }

    private static void ValidateSessions(string connection)
    {
        using (var db = new CMSDatabase(Options(connection)))
        { db.Database.OpenConnection(); SessionStorageCompatibility.Validate(db); }
    }

    private static async Task SessionChecks()
    {
        SessionUnitChecks();
        Check(Count(serverConnection, "SELECT COUNT(*) FROM information_schema.schemata WHERE schema_name='test'") == 0, "temporary server test database must be absent before reservation");
        Execute(serverConnection, "CREATE DATABASE test CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci");
        ownedDatabases.Add("test");
        string connection = ForDatabase("test");
        using (var db = new CMSDatabase(Options(connection))) MySqlSecurityInstallation.Initialize(db);
        ValidateSessions(connection);
        Check(true, "new session schema creates on utf8mb4 and passes storage validation");
        using (var db = new CMSDatabase(Options(connection)))
        {
            var user = db.Users.Single();
            DatabaseInteraction.ChangeInitialPassword(db, user, new UserModel { CurrentPassword = "admin", NewPassword = NewPassword, ConfirmPassword = NewPassword }, out int result);
            Check(result == 200, "isolated test administrator initialized");
        }
        string token = Issue(connection);
        SessionToken.TryHash(token, out string digest);
        using (var db = new CMSDatabase(Options(connection)))
            Check(db.ConnectedUsers.Single().TokenHash == digest && digest != token, "database stores digest only");
        Check(Authenticate(connection, digest, DateTime.UtcNow) == null, "database hash cannot be replayed as cookie");
        Check(Authenticate(connection, token, DateTime.UtcNow) != null, "valid bearer token authenticates");
        var epoch = new DateTime(2026, 10, 16, 0, 0, 0, DateTimeKind.Utc);
        using (var db = new CMSDatabase(Options(connection))) db.Database.ExecuteSqlCommand("UPDATE ConnectedUsers SET LastActionUtc={0}", epoch);
        for (int i = 1; i <= 180; i++)
            if (Authenticate(connection, token, epoch.AddMinutes(i * 9)) == null) throw new InvalidOperationException("CHECK: sliding session stopped");
        Check(true, "actual database session slides for 27 hours without absolute cap");
        DateTime last = epoch.AddMinutes(180 * 9);
        Check(Authenticate(connection, token, last.AddMinutes(1), prefetch: true) != null, "prefetch can read while authenticated");
        using (var db = new CMSDatabase(Options(connection))) Check(db.ConnectedUsers.Single().LastActionUtc == last, "prefetch does not change LastActionUtc");
        Check(Authenticate(connection, token, last.AddMinutes(10)) == null && Count(connection, "SELECT COUNT(*) FROM ConnectedUsers") == 0, "exact idle timeout deletes server record");
        string first = Issue(connection), second = Issue(connection);
        Check(Authenticate(connection, first, DateTime.UtcNow) == null && Authenticate(connection, second, DateTime.UtcNow) != null, "second login revokes first session");
        using (var db = new CMSDatabase(Options(connection)))
        {
            DatabaseInteraction.Exit(db, null, SessionContext(first), out int code);
            Check(code == 200 && db.ConnectedUsers.Count() == 1, "stale logout cannot delete replacement session");
            var exit = SessionContext(second);
            DatabaseInteraction.Exit(db, null, exit, out code);
            Check(code == 200 && db.ConnectedUsers.Count() == 0 && exit.Response.Headers["Set-Cookie"].Count == 2, "logout revokes token and clears only legacy cookies");
            DatabaseInteraction.Exit(db, null, SessionContext(second), out code);
            Check(code == 200, "repeated logout is idempotent");
        }
        token = Issue(connection);
        using (var db = new CMSDatabase(Options(connection)))
        {
            var context = SessionContext(token);
            context.Connection.RemoteIpAddress = IPAddress.Parse("2001:db8::1"); context.Request.Headers["User-Agent"] = "changed";
            Check(SecurityFunctions.CheckCookies(db, context) != null, "IP and User-Agent changes do not revoke session");
            context.Request.Scheme = "http";
            Check(SecurityFunctions.CheckCookies(db, context) == null, "HTTP request cannot authenticate");
        }
        using (var db = new CMSDatabase(Options(connection)))
        {
            var context = SessionContext(token); var user = SecurityFunctions.CheckCookies(db, context); context.Items["User"] = user;
            var profile = new UserModel { ID = user.ID, Login = user.Login, IdleTime = 20, CurrentPassword = NewPassword, Email = "changed@example.invalid" };
            DatabaseInteraction.EditUserData(db, profile, context, out int code);
            Check(code == 200 && db.ConnectedUsers.Count() == 1, "profile without password preserves session");
            profile.NewPassword = "Replacement-session-password!";
            DatabaseInteraction.EditUserData(db, profile, context, out code);
            Check(code == 200 && db.ConnectedUsers.Count() == 0 && SecurityFunctions.VerifyPassword(db.Users.Single(), profile.NewPassword), "profile password and revocation committed together");
            Check(context.Response.Headers["Set-Cookie"].Count == 2, "password change clears only legacy cookies");
        }
        Check(Authenticate(connection, token, DateTime.UtcNow) == null, "copied token rejected after password change");
        SessionFailureChecks(connection);
        await HttpGuardChecks(connection);
        await SessionRaceChecks();
        SessionUpgradeChecks();
    }

    private static async Task HttpGuardChecks(string connection)
    {
        HttpConnection = connection; HttpRoot = Path.Combine(sourceRoot, "StoreCMS");
        foreach (bool trusted in new[] { false, true })
        {
            using (var host = new WebHostBuilder().UseKestrel().UseUrls("http://127.0.0.1:0").UseContentRoot(HttpRoot)
                .UseSetting(WebHostDefaults.ApplicationKey, typeof(Treynessen.Controllers.AdminPanelController).Assembly.GetName().Name)
                .ConfigureServices(services => new SecurityTestStartup().ConfigureServices(services))
                .Configure(app =>
                {
                    app.Use((context, next) => { context.Connection.RemoteIpAddress = trusted ? IPAddress.Loopback : IPAddress.Parse("203.0.113.10"); return next(); });
                    app.UseForwardedHeaders();
                    new SecurityTestStartup().Configure(app);
                }).Build())
            {
                host.Start();
                using (var client = new HttpClient { BaseAddress = new Uri(host.ServerFeatures.Get<IServerAddressesFeature>().Addresses.Single()) })
                {
                    var direct = await client.GetAsync("/private/control-room");
                    Check(direct.StatusCode == HttpStatusCode.Forbidden && direct.Headers.CacheControl.NoStore, "plain HTTP administrative response forbidden and not cached");
                    client.DefaultRequestHeaders.Add("X-Forwarded-Proto", "https");
                    var forwarded = await client.GetAsync("/private/control-room");
                    Check(forwarded.StatusCode == (trusted ? HttpStatusCode.OK : HttpStatusCode.Forbidden), trusted ? "trusted loopback proxy supplies HTTPS scheme" : "untrusted forwarded HTTPS header cannot bypass guard");
                }
            }
        }
    }

    private static void SessionFailureChecks(string connection)
    {
        string token = Issue(connection);
        Execute(connection, "CREATE TRIGGER deny_session_insert BEFORE INSERT ON ConnectedUsers FOR EACH ROW SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT='isolated failure injection'");
        try
        {
            using (var db = new CMSDatabase(Options(connection)))
            {
                var context = SessionContext(); bool failed = false;
                try { DatabaseInteraction.AddConnectedUser(db, db.Users.Single(), context); }
                catch (DbUpdateException) { failed = true; }
                Check(failed && !context.Response.Headers.ContainsKey("Set-Cookie"), "insert failure never issues cookie before commit");
            }
            Check(Authenticate(connection, token, DateTime.UtcNow) != null, "failed replacement rolls back deletion of former session");
        }
        finally { Execute(connection, "DROP TRIGGER deny_session_insert"); }
        Execute(connection, "CREATE TRIGGER deny_session_delete BEFORE DELETE ON ConnectedUsers FOR EACH ROW SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT='isolated failure injection'");
        try
        {
            string before;
            using (var db = new CMSDatabase(Options(connection)))
            {
                var context = SessionContext(token); var user = SecurityFunctions.CheckCookies(db, context); context.Items["User"] = user; before = user.Password;
                bool failed = false;
                try
                {
                    DatabaseInteraction.EditUserData(db, new UserModel { ID = user.ID, Login = user.Login, IdleTime = user.IdleTime, CurrentPassword = "Replacement-session-password!", NewPassword = "Failure-test-password!" }, context, out int code);
                }
                catch (MySql.Data.MySqlClient.MySqlException) { failed = true; }
                Check(failed && !context.Response.Headers.ContainsKey("Set-Cookie"), "revocation failure does not clear cookie or claim success");
            }
            using (var db = new CMSDatabase(Options(connection))) Check(db.Users.Single().Password == before && db.ConnectedUsers.Count() == 1, "password and session both preserved after revocation rollback");
            using (var db = new CMSDatabase(Options(connection)))
            {
                bool failed = false;
                try { DatabaseInteraction.Exit(db, null, SessionContext(token), out int code); }
                catch (MySql.Data.MySqlClient.MySqlException) { failed = true; }
                Check(failed && db.ConnectedUsers.Count() == 1, "logout database failure never claims server revocation");
            }
        }
        finally { Execute(connection, "DROP TRIGGER deny_session_delete"); }
    }

    private static async Task SessionRaceChecks()
    {
        string name = CreateDatabase(), connection = ForDatabase(name);
        using (var db = new CMSDatabase(Options(connection))) MySqlSecurityInstallation.Initialize(db);
        var processes = Enumerable.Range(0, 2).Select(_ => Task.Run(() =>
        {
            var info = new ProcessStartInfo("dotnet") { UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true,
                Arguments = "\"" + Assembly.GetExecutingAssembly().Location + "\" --session-worker --credential-config \"" + credentialPath + "\" --database " + name };
            using (var process = Process.Start(info)) { process.StandardOutput.ReadToEnd(); process.StandardError.ReadToEnd(); process.WaitForExit(); return process.ExitCode; }
        })).ToArray();
        Check((await Task.WhenAll(processes)).All(x => x == 0) && Count(connection, "SELECT COUNT(*) FROM ConnectedUsers") == 1, "two real processes issue one current session without unique conflict");
        using (var stale = new CMSDatabase(Options(connection)))
        using (var current = new CMSDatabase(Options(connection)))
        {
            var oldUser = stale.Users.Single(); var user = current.Users.Single();
            DatabaseInteraction.ChangeInitialPassword(current, user, new UserModel { CurrentPassword = "admin", NewPassword = NewPassword, ConfirmPassword = NewPassword }, out int code);
            var context = SessionContext();
            Check(code == 200 && !DatabaseInteraction.AddConnectedUser(stale, oldUser, context) && !context.Response.Headers.ContainsKey("Set-Cookie"), "stale password verification cannot issue session after competing change");
        }
        string token = Issue(connection);
        var results = Enumerable.Range(0, 12).Select(_ => Task.Run(() =>
        {
            using (var db = new CMSDatabase(Options(connection))) return SecurityFunctions.CheckCookies(db, SessionContext(token));
        })).ToArray();
        Check((await Task.WhenAll(results)).All(x => x != null), "parallel renewals stay authenticated with UTC sampled after lock");
        using (var db = new CMSDatabase(Options(connection)))
        {
            using (var transaction = db.Database.BeginTransaction(IsolationLevel.ReadCommitted))
            { SessionStore.LockUser(db, db.Users.Single().ID); SessionStore.RevokeAll(db, db.Users.Single().ID); }
            Check(db.ConnectedUsers.Count() == 1, "rollback preserves session after uncommitted revocation");
        }
    }

    private static void SessionUpgradeChecks()
    {
        string connection = ForDatabase(CreateDatabase());
        using (var db = new CMSDatabase(Options(connection))) MySqlSecurityInstallation.Initialize(db);
        Execute(connection, "ALTER TABLE ConnectedUsers DROP INDEX IX_ConnectedUsers_TokenHash, DROP COLUMN TokenHash, CHANGE LastActionUtc LastActionTime datetime(6) NOT NULL, MODIFY IPAdress varchar(255) NOT NULL, ADD UserName varchar(255) NOT NULL, ADD LoginKey varchar(255) NOT NULL, ADD UNIQUE INDEX legacy_session_index(UserName,LoginKey,IPAdress,UserAgent)");
        Execute(connection, "INSERT INTO ConnectedUsers(UserID,UserName,LoginKey,IPAdress,UserAgent,LastActionTime) SELECT ID,'legacy-selector','legacy-secret','127.0.0.1','fixture',UTC_TIMESTAMP(6) FROM Users");
        string password;
        using (var db = new CMSDatabase(Options(connection))) { password = db.Users.Single().Password; Check(Refuses(() => MySqlSecurityInstallation.Initialize(db)), "old session schema refuses startup without bootstrap"); }
        Execute(connection, "UPDATE Users SET Password=REPEAT('a',32), RequirePasswordChange=0");
        Execute(connection, "UPDATE SecurityInstallation SET LegacyPasswordResetCompleted=0");
        using (var db = new CMSDatabase(Options(connection)))
        {
            LegacyPasswordReplacement.Replace(db, db.Users.Single().ID, NewPassword);
            password = db.Users.Single().Password;
            Check(SecurityFunctions.VerifyPassword(db.Users.Single(), NewPassword) && db.SecurityInstallation.Single().LegacyPasswordResetCompleted
                && Count(connection, "SELECT COUNT(*) FROM ConnectedUsers") == 0, "legacy utility still atomically revokes actual seven-column sessions before V2 upgrade");
        }
        Execute(connection, "INSERT INTO ConnectedUsers(UserID,UserName,LoginKey,IPAdress,UserAgent,LastActionTime) SELECT ID,'legacy-selector','legacy-secret','127.0.0.1','fixture',UTC_TIMESTAMP(6) FROM Users");
        ApplySecurityUpgrade(connection, "tools/sql/UpgradeSessionsV2.sql");
        ValidateSessions(connection);
        using (var db = new CMSDatabase(Options(connection))) Check(db.ConnectedUsers.Count() == 0 && db.Users.Single().Password == password && db.SecurityInstallation.Count() == 1, "real V2 SQL revokes old sessions and preserves password and marker");
        string token = Issue(connection);
        ApplySecurityUpgrade(connection, "tools/sql/UpgradeSessionsV2.sql");
        Check(Authenticate(connection, token, DateTime.UtcNow) != null, "repeat SQL preserves new session");
        Execute(connection, "ALTER TABLE ConnectedUsers MODIFY TokenHash char(64) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL");
        Check(Refuses(() => ValidateSessions(connection)), "runtime refuses nonbinary hash collation");
        string snapshot = StorageSnapshot(connection);
        bool refused = false;
        try { ApplySecurityUpgrade(connection, "tools/sql/UpgradeSessionsV2.sql"); }
        catch (MySql.Data.MySqlClient.MySqlException error) { refused = error.Number == 1644; }
        finally { Execute(connection, "DROP PROCEDURE IF EXISTS StoreCMS_UpgradeSessionsV2"); }
        Check(refused && StorageSnapshot(connection) == snapshot, "SQL refuses partial schema before changing any data");
        Execute(connection, "DELETE FROM ConnectedUsers");
        Execute(connection, "ALTER TABLE ConnectedUsers MODIFY TokenHash char(63) CHARACTER SET ascii COLLATE ascii_bin NOT NULL");
        Check(Refuses(() => ValidateSessions(connection)), "runtime refuses incompatible hash storage");
        Execute(connection, "ALTER TABLE ConnectedUsers MODIFY TokenHash char(64) CHARACTER SET ascii COLLATE ascii_bin NOT NULL");
        Execute(connection, "RENAME TABLE ConnectedUsers TO original_sessions");
        Execute(connection, "CREATE TABLE ConnectedUsers LIKE original_sessions");
        Execute(connection, "ALTER TABLE ConnectedUsers ENGINE=MyISAM");
        Check(Refuses(() => ValidateSessions(connection)), "runtime refuses nontransactional session storage");
        refused = false;
        try { ApplySecurityUpgrade(connection, "tools/sql/UpgradeSessionsV2.sql"); }
        catch (MySql.Data.MySqlClient.MySqlException error) { refused = error.Number == 1644; }
        finally { Execute(connection, "DROP PROCEDURE IF EXISTS StoreCMS_UpgradeSessionsV2"); }
        Check(refused, "SQL refuses MyISAM before any upgrade");
    }
}
