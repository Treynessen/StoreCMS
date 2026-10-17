using System;
using System.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Treynessen.Security;
using Treynessen.Localization;
using Treynessen.LogManagement;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;

namespace Treynessen.Database
{
    public static partial class DatabaseInteraction
    {
        public static bool AddConnectedUser(CMSDatabase db, User user, HttpContext context)
        {
            if (!context.Request.IsHttps || user == null) return false;
            string token = SessionToken.Create();
            SessionToken.TryHash(token, out string hash);
            using (var transaction = db.Database.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                var current = SessionStore.LockUser(db, user.ID);
                // [Комментарий ИИ-агента]: Проверка пароля до блокировки не разрешает выдать сессию после его конкурентной смены.
                if (current == null || !string.Equals(current.Password, user.Password, StringComparison.Ordinal)) return false;
                SessionStore.RevokeAll(db, user.ID);
                string agent = context.Request.Headers["User-Agent"].ToString();
                db.ConnectedUsers.Add(new ConnectedUser
                {
                    TokenHash = hash, LastActionUtc = DateTime.UtcNow, UserID = user.ID,
                    IPAdress = context.Connection.RemoteIpAddress?.ToString() ?? "",
                    UserAgent = agent.Length > 255 ? agent.Substring(0, 255) : agent
                });
                db.SaveChanges();
                transaction.Commit();
            }
            SessionCookies.Write(context, token);
            LogManagementFunctions.TryAddCommittedAdminPanelLog(db, context,
                $"{(context.Items["LogLocalization"] as IAdminPanelLogLocalization)?.LoggedIn}. IP: {context.Connection.RemoteIpAddress}", user);
            return true;
        }
    }
}
