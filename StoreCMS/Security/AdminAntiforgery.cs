using System;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Treynessen.Security
{
    public sealed class AdminAntiforgery : IAntiforgeryAdditionalDataProvider
    {
        public const string CookieName = "StoreCMS.Antiforgery.v1";
        public const string HeaderName = "X-CSRF-TOKEN";
        public const string FieldName = "__RequestVerificationToken";

        public static void Configure(IServiceCollection services)
        {
            services.AddAntiforgery(options =>
            {
                options.HeaderName = HeaderName;
                options.FormFieldName = FieldName;
                options.Cookie.Name = CookieName;
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.Path = "/";
                options.Cookie.IsEssential = true;
            });
            services.AddSingleton<IAntiforgeryAdditionalDataProvider, AdminAntiforgery>();
        }

        // [Комментарий ИИ-агента]: Хеш предъявленной cookie остаётся внутри защищённого токена; БД и скользящий срок сессии здесь не затрагиваются.
        public string GetAdditionalData(HttpContext context) =>
            SessionToken.TryHash(context.Request.Cookies[SessionCookies.Name], out string hash)
                ? "session-v2:" + hash : "anonymous-v1";

        public bool ValidateAdditionalData(HttpContext context, string additionalData) =>
            string.Equals(GetAdditionalData(context), additionalData, StringComparison.Ordinal);
    }
}
