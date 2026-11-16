using System;
using Microsoft.AspNetCore.Http;
using Treynessen.Database.Entities;

namespace Treynessen.Security
{
    public interface ISessionPolicy
    {
        bool IsExpired(ConnectedUser session, User user, DateTime utcNow);
        bool ShouldRenew(HttpContext context);
    }

    public sealed class AdminSessionPolicy : ISessionPolicy
    {
        public static readonly AdminSessionPolicy Instance = new AdminSessionPolicy();

        public bool IsExpired(ConnectedUser session, User user, DateTime utcNow)
        {
            var last = DateTime.SpecifyKind(session.LastActionUtc, DateTimeKind.Utc);
            return user.IdleTime < 10 || user.IdleTime > 10080 || last > utcNow
                || utcNow - last >= TimeSpan.FromMinutes(user.IdleTime);
        }

        public bool ShouldRenew(HttpContext context)
        {
            // [Комментарий ИИ-агента]: Однократная подгрузка журнала относится к открытию страницы и продлевает сессию, как ручная загрузка; предзагрузка и служебные методы не продлевают её.
            string method = context.Request.Method;
            string purpose = context.Request.Headers["Purpose"].ToString() + " " + context.Request.Headers["Sec-Purpose"].ToString();
            return (method == "GET" || method == "POST" || method == "PUT" || method == "DELETE")
                && purpose.IndexOf("prefetch", StringComparison.OrdinalIgnoreCase) < 0;
        }
    }
}
