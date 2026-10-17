using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Treynessen.Security;

internal static partial class AntiforgeryRegression
{
    static bool expectF02;
    static async Task UserValidationFlows(HttpClient client, string token)
    {
        client.DefaultRequestHeaders.Add(AdminAntiforgery.HeaderName, token);
        try
        {
            string initialUsers = Scalar("SELECT COUNT(*) FROM Users");
            using (var response = await Send(client, "POST", "/admin", "42", "UserModel.Login", "admin2",
                "UserModel.NewPassword", "admin", "UserModel.UserTypeId", "1"))
            {
                Check((int)response.StatusCode == 422, "F-02 password rejected by actual controller");
                string body = await response.Content.ReadAsStringAsync();
                if (expectF02)
                {
                    Check(string.IsNullOrEmpty(body), "F-02 reproduced: HTTP 422 has no validation cause");
                    Console.WriteLine("F-02 reproduced: forbidden default password; HTTP 422 without error code.");
                    return;
                }
                Check(JObject.Parse(body).Value<string>("error") == "user_password_invalid", "F-02 specific safe password code");
            }
            Check(Scalar("SELECT COUNT(*) FROM Users") == initialUsers, "F-02 rejection never creates user");
            Check(Treynessen.Database.DatabaseInteraction.CorrectPassword("abcde"), "policy does not require mixing character classes");
            long beforeUsers = long.Parse(Scalar("SELECT COUNT(*) FROM Users"));
            async Task Error(string page, int status, string code, params string[] fields)
            {
                using (var response = await Send(client, "POST", "/admin", page, fields))
                {
                    Check((int)response.StatusCode == status, "user validation preserves HTTP status");
                    string body = await response.Content.ReadAsStringAsync();
                    var payload = JObject.Parse(body);
                    Check(payload.Count == 1 && payload.Value<string>("error") == code, "only safe structured error code returned");
                    Check(response.Content.Headers.ContentType.MediaType == "application/json", "error content type");
                    Check(receivedHeader == token, "user request keeps CSRF header");
                    Check(response.Headers.Location == null && response.Headers.CacheControl.NoStore, "no redirect and no-store on user failure");
                }
            }
            foreach (string rejected in new[] { "ADMIN", "abcd", "abc de", "пароль" })
                await Error("42", 422, "user_password_invalid", "UserModel.Login", "admin2", "UserModel.NewPassword", rejected, "UserModel.UserTypeId", "1");
            await Error("42", 422, "user_login_required", "UserModel.Login", "", "UserModel.NewPassword", password, "UserModel.UserTypeId", "1");
            await Error("42", 422, "user_login_invalid", "UserModel.Login", "bad login", "UserModel.NewPassword", password, "UserModel.UserTypeId", "1");
            await Error("42", 422, "user_password_required", "UserModel.Login", "admin2", "UserModel.NewPassword", "", "UserModel.UserTypeId", "1");
            await Error("42", 422, "user_role_required", "UserModel.Login", "admin2", "UserModel.NewPassword", password);
            await Error("42", 422, "user_role_invalid", "UserModel.Login", "admin2", "UserModel.NewPassword", password, "UserModel.UserTypeId", "999");
            await Error("42", 422, "user_data_required");
            Check(long.Parse(Scalar("SELECT COUNT(*) FROM Users")) == beforeUsers, "rejected creation has no user write");
            await TokenStatusWithoutAddedHeader(client, "42", 201, "UserModel.Login", "admin2", "UserModel.NewPassword", password, "UserModel.UserTypeId", "1");
            string id = Scalar("SELECT ID FROM Users WHERE Login='admin2'");
            Check(!string.IsNullOrEmpty(id), "valid password creates isolated user");
            Check(SecurityFunctions.VerifyPassword(new Treynessen.Database.Entities.User {
                Password = Scalar("SELECT Password FROM Users WHERE Login='admin2'")
            }, password), "created user password hash verifies without disclosure");
            await HtmlToken(client, "/admin?pageID=8");
            await Error("42", 409, "user_login_taken", "UserModel.Login", "admin2", "UserModel.NewPassword", password, "UserModel.UserTypeId", "1");
            await Error("43", 422, "user_id_required", "UserModel.UserTypeId", "1");
            await Error("43", 422, "user_role_required", "itemID", id, "UserModel.Login", "ignored");
            await Error("43", 422, "user_role_invalid", "itemID", id, "UserModel.UserTypeId", "999");
            await TokenStatusWithoutAddedHeader(client, "43", 200, "itemID", id, "UserModel.UserTypeId", "1");
            await Error("52", 422, "user_password_invalid", "UserModel.ID", "1", "UserModel.Login", "fixture", "UserModel.NewPassword", "admin", "UserModel.IdleTime", "10");
            await Error("52", 422, "user_login_required", "UserModel.ID", "1", "UserModel.Login", "", "UserModel.IdleTime", "10");
            await Error("52", 422, "user_idle_time_invalid", "UserModel.ID", "1", "UserModel.Login", "fixture", "UserModel.IdleTime", "9");
            await Error("52", 422, "user_id_required", "UserModel.Login", "fixture", "UserModel.IdleTime", "10");
            await Error("52", 422, "user_data_required");
            await Error("52", 409, "user_login_taken", "UserModel.ID", "1", "UserModel.Login", "admin2", "UserModel.CurrentPassword", password, "UserModel.IdleTime", "10");
            // [Комментарий ИИ-агента]: Удаляется только собственная фикстура из TEMPORARY-таблиц удерживаемого соединения.
            Sql("DELETE FROM AdminPanelLogs WHERE UserId=" + id);
            Sql("DELETE FROM Users WHERE ID=" + id);
        }
        finally { client.DefaultRequestHeaders.Remove(AdminAntiforgery.HeaderName); }
    }
    static Task TokenStatusWithoutAddedHeader(HttpClient client, string page, int status, params string[] fields) =>
        Status(client, "POST", "/admin", page, status, fields);
}
