using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Treynessen.AdminPanelTypes;
using Treynessen.Security;

internal static partial class AntiforgeryRegression
{
    static int boundaryCalls;
    sealed class MutationBoundaryFilter : IActionFilter, IOrderedFilter
    {
        public int Order => int.MaxValue;
        public void OnActionExecuting(ActionExecutingContext context)
        {
            // [Комментарий ИИ-агента]: Только в тестовом host маркер заменяет action после production-защит; позволяет проверить каждую пару без выполнения CRUD и файловых изменений.
            if (context.HttpContext.Request.Headers["X-Test-Mutation-Boundary"] == "1")
            {
                boundaryCalls++;
                context.Result = new StatusCodeResult(204);
            }
        }
        public void OnActionExecuted(ActionExecutedContext context) { }
    }
    static async Task<string> CurrentToken(HttpClient client)
    {
        using (var response = await client.GetAsync("/admin?pageID=9"))
        {
            string html = await response.Content.ReadAsStringAsync();
            return WebUtility.HtmlDecode(Regex.Match(html, "<meta name=\"csrf-token\" content=\"([^\"]+)\"").Groups[1].Value);
        }
    }
    static string MutationSnapshot() => Scalar("SELECT GROUP_CONCAT(CONCAT(ID,':',Login,':',Password,':',RequirePasswordChange,':',UserTypeID) ORDER BY ID) FROM Users")
        + "|" + Scalar("SELECT GROUP_CONCAT(CONCAT(ID,':',Name,':',AccessLevel) ORDER BY ID) FROM UserTypes")
        + "|" + Scalar("SELECT GROUP_CONCAT(CONCAT(ID,':',TokenHash,':',UserID) ORDER BY ID) FROM ConnectedUsers")
        + "|" + Scalar("SELECT COUNT(*) FROM AdminPanelLogs");

    static async Task RawMutation(HttpClient client, string path, string method, string page, string token,
        int expected, bool boundary = true, bool hidden = false, string cookie = null)
    {
        int beforeCalls = boundaryCalls;
        string beforeData = MutationSnapshot();
        using (var request = new HttpRequestMessage(new HttpMethod(method), path))
        using (var form = new MultipartFormDataContent())
        {
            form.Add(new StringContent(page), "pageID");
            // [Комментарий ИИ-агента]: Полезная нагрузка содержит валидные данные роли, credentials и файл; при отказе guard action не должен начаться даже с пригодным телом.
            form.Add(new StringContent("csrf-must-not-create"), "UserTypeModel.Name");
            form.Add(new StringContent("2"), "UserTypeModel.AccessLevel");
            form.Add(new StringContent("fixture"), "Login");
            form.Add(new StringContent(password), "Password");
            form.Add(new ByteArrayContent(new byte[] {137,80,78,71,13,10,26,10}), "uploadedFile", "fixture.png");
            if (token != null)
            {
                if (hidden) form.Add(new StringContent(token), AdminAntiforgery.FieldName);
                else request.Headers.TryAddWithoutValidation(AdminAntiforgery.HeaderName, token);
            }
            if (cookie != null) request.Headers.Add("Cookie", cookie);
            if (boundary) request.Headers.Add("X-Test-Mutation-Boundary", "1");
            request.Content = form;
            requests++;
            using (var response = await client.SendAsync(request))
            {
                Check((int)response.StatusCode == expected, "enforcement " + method + " " + page + " expected " + expected + " got " + (int)response.StatusCode);
                Check(response.Headers.Location == null, "CSRF/authorization never redirects");
                Check(response.Headers.CacheControl != null && response.Headers.CacheControl.NoStore, "enforcement no-store");
                if (expected == 400) Check((await response.Content.ReadAsByteArrayAsync()).Length == 0, "CSRF 400 has no internal details");
            }
        }
        Check(boundaryCalls == beforeCalls + (expected == 204 ? 1 : 0), "action boundary reached only after valid CSRF");
        Check(MutationSnapshot() == beforeData, "no business mutation on guard rejection or test boundary");
    }
    static async Task EnforcementFlows(HttpClient client, CookieContainer jar, Uri origin)
    {
        string anonymous = await CurrentToken(client);
        await RawMutation(client, "/admin", "POST", "54", null, 400, false);
        await RawMutation(client, "/admin", "DELETE", "58", null, 400, false);
        await RawMutation(client, "/admin", "POST", "46", "broken", 401);
        await RawMutation(client, "/admin", "POST", "54", anonymous, 204);
        await TokenStatus(client, anonymous, "POST", "54", 200, "Login", "fixture", "Password", password);
        string token = await CurrentToken(client);
        var operations = new[] {
            new { Method = "POST", Pages = new[]{13,14,16,18,20,21,26,27,29,30,32,33,36,37,38,39,40,42,43,46,47,49,50,52,53,54,59}},
            new { Method = "PUT", Pages = new[]{24,35}},
            new { Method = "DELETE", Pages = new[]{15,17,22,25,28,31,34,41,44,48,51,58}}
        };
        Check(operations.Sum(x => x.Pages.Length) == 41, "exact 41 operations");
        var support = typeof(Treynessen.Controllers.AdminPanelController).GetMethod("SupportsAdminPanelMutation", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        foreach (var pair in operations)
            foreach (AdminPanelPages page in Enum.GetValues(typeof(AdminPanelPages)))
                Check((bool)support.Invoke(null, new object[]{(AdminPanelPages?)page, pair.Method}) == pair.Pages.Contains((int)page), "matrix equals production method selector");
        foreach (var pair in operations)
        {
            var action = typeof(Treynessen.Controllers.AdminPanelController).GetMethods()
                .Single(m => m.Name == "AdminPanel" && m.GetCustomAttributes(true).Any(a => a.GetType().Name == "Http" + pair.Method.Substring(0,1) + pair.Method.Substring(1).ToLowerInvariant() + "Attribute"));
            Check(action != null, "real MVC action found for method");
            foreach (int page in pair.Pages)
            {
                Check(Enum.IsDefined(typeof(AdminPanelPages), page), "known enum operation");
                foreach (string path in new[]{"/admin", "/private/control-room"})
                {
                    foreach (string invalid in new[]{null,"","broken", token.Substring(0,token.Length-3)+"AAA", anonymous})
                        await RawMutation(client, path, pair.Method, page.ToString(), invalid, 400);
                    await RawMutation(client, path, pair.Method, page.ToString(), token, 204);
                    await RawMutation(client, path, pair.Method, page.ToString(), token, 204, true, true);
                    // [Комментарий ИИ-агента]: Без тестового перехватчика отказ также проверяется до настоящего action для каждой операции.
                    await RawMutation(client, path, pair.Method, page.ToString(), null, 400, false);
                }
            }
        }
        await FileMutationFlows(client, token);
        string session = jar.GetCookies(origin)[SessionCookies.Name].Value;
        var otherJar = new CookieContainer();
        otherJar.Add(origin, new Cookie(SessionCookies.Name, session, "/") { Secure = true });
        using (var other = new HttpClient(new HttpClientHandler {
            CookieContainer = otherJar, AllowAutoRedirect = false,
            // [Комментарий ИИ-агента]: Исключение сертификата действует только в локальном одноразовом тестовом host.
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        }) { BaseAddress = origin })
        {
            foreach (var pair in operations)
                foreach (int page in pair.Pages)
                    await RawMutation(other, "/admin", pair.Method, page.ToString(), token, 400);
            await CurrentToken(other);
            foreach (var pair in operations)
                foreach (int page in pair.Pages)
                    await RawMutation(other, "/admin", pair.Method, page.ToString(), token, 400);
        }
        Sql("UPDATE UserTypes SET AccessLevel=1 WHERE ID=1");
        foreach (var pair in operations)
            foreach (int page in pair.Pages.Where(p => p != 54 && p != 58 && p != 59))
                await RawMutation(client, "/admin", pair.Method, page.ToString(), "broken", 403);
        Sql("UPDATE UserTypes SET AccessLevel=5 WHERE ID=1");
        Sql("UPDATE Users SET RequirePasswordChange=1 WHERE ID=1");
        await RawMutation(client, "/admin", "POST", "46", null, 403);
        await RawMutation(client, "/admin", "POST", "59", null, 400);
        await RawMutation(client, "/admin", "POST", "59", token, 204);
        await RawMutation(client, "/admin", "DELETE", "58", null, 400);
        Sql("UPDATE Users SET RequirePasswordChange=0 WHERE ID=1");
        Sql("UPDATE ConnectedUsers SET LastActionUtc=UTC_TIMESTAMP()-INTERVAL 11 MINUTE");
        // [Комментарий ИИ-агента]: Первый запрос удаляет только истёкшую TEMPORARY-сессию; снимок бизнес-данных берётся после этого служебного отзыва.
        await Status(client, "POST", "/admin", "46", 401);
        await RawMutation(client, "/admin", "POST", "46", "broken", 401);
        await RawMutation(client, "/admin", "DELETE", "58", "broken", 400);
        jar.GetCookies(origin)[SessionCookies.Name].Expired = true;
        Console.WriteLine("Enforcement: all 41 pairs, both routes, header/hidden multipart, invalid tokens and cookie mismatch passed; boundary=" + boundaryCalls);
    }
}
