using Microsoft.AspNetCore.Http;

namespace Treynessen.Security
{
    public static class SessionCookies
    {
        public const string Name = "StoreCMS.Session.v2";
        public static CookieOptions Options() => new CookieOptions
        {
            HttpOnly = true, Secure = true, SameSite = SameSiteMode.Lax, Path = "/", IsEssential = true
        };

        public static void Write(HttpContext context, string token)
        {
            if (!context.Request.IsHttps) throw new System.InvalidOperationException("Для входа в панель требуется HTTPS.");
            ClearLegacy(context);
            context.Response.Cookies.Append(Name, token, Options());
        }

        // [Комментарий ИИ-агента]: Общую auth-cookie не удаляем: запоздавший ответ мог бы стереть cookie нового входа. Доступ прекращается отзывом токена в БД.
        public static void ClearLegacy(HttpContext context)
        {
            context.Response.Cookies.Delete("userName", Options());
            context.Response.Cookies.Delete("loginKey", Options());
        }

    }
}
