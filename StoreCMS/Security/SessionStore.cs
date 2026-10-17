using System;
using System.Data;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;

namespace Treynessen.Security
{
    public static class SessionStore
    {
        // [Комментарий ИИ-агента]: Единый порядок блокировок Users → ConnectedUsers действует и между экземплярами приложения.
        public static User LockUser(CMSDatabase db, int userId)
        {
            if (db.Database.CurrentTransaction == null) throw new InvalidOperationException("Требуется транзакция сессии.");
            using (var command = db.Database.GetDbConnection().CreateCommand())
            {
                command.Transaction = db.Database.CurrentTransaction.GetDbTransaction();
                command.CommandText = "SELECT ID FROM Users WHERE ID=@id FOR UPDATE";
                var parameter = command.CreateParameter(); parameter.ParameterName = "@id"; parameter.Value = userId;
                command.Parameters.Add(parameter);
                if (command.ExecuteScalar() == null) return null;
            }
            return db.Users.AsNoTracking().Include(u => u.UserType).SingleOrDefault(u => u.ID == userId);
        }

        public static User Authenticate(CMSDatabase db, HttpContext context, ISessionPolicy policy, Func<DateTime> utcClock)
        {
            if (!context.Request.IsHttps) return null;
            if (!SessionToken.TryHash(context.Request.Cookies[SessionCookies.Name], out string hash))
            {
                if (context.Request.Cookies.ContainsKey(SessionCookies.Name)
                    || context.Request.Cookies.ContainsKey("userName") || context.Request.Cookies.ContainsKey("loginKey")) SessionCookies.ClearLegacy(context);
                return null;
            }
            var candidate = db.ConnectedUsers.AsNoTracking().SingleOrDefault(s => s.TokenHash == hash);
            if (candidate == null) { SessionCookies.ClearLegacy(context); return null; }
            using (var transaction = db.Database.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                var user = LockUser(db, candidate.UserID);
                var session = db.ConnectedUsers.AsNoTracking().SingleOrDefault(s => s.TokenHash == hash);
                if (user == null || session == null) { SessionCookies.ClearLegacy(context); return null; }
                DateTime utcNow = utcClock();
                if (policy.IsExpired(session, user, utcNow))
                {
                    Delete(db, hash);
                    transaction.Commit(); SessionCookies.ClearLegacy(context); return null;
                }
                if (policy.ShouldRenew(context))
                    db.Database.ExecuteSqlCommand("UPDATE ConnectedUsers SET LastActionUtc={0} WHERE ID={1} AND TokenHash={2} AND LastActionUtc<={0}", utcNow, session.ID, hash);
                var tracked = db.Users.Include(u => u.UserType).Single(u => u.ID == user.ID);
                db.Entry(tracked).Reload();
                db.Entry(tracked).Reference(u => u.UserType).Load();
                transaction.Commit();
                return tracked;
            }
        }

        public static bool IsCurrent(CMSDatabase db, HttpContext context, User user, ISessionPolicy policy, DateTime utcNow)
        {
            if (!SessionToken.TryHash(context.Request.Cookies[SessionCookies.Name], out string hash)) return false;
            var session = db.ConnectedUsers.AsNoTracking().SingleOrDefault(s => s.TokenHash == hash && s.UserID == user.ID);
            return session != null && !policy.IsExpired(session, user, utcNow);
        }

        public static void Delete(CMSDatabase db, string hash) =>
            db.Database.ExecuteSqlCommand("DELETE FROM ConnectedUsers WHERE TokenHash={0}", hash);

        public static void RevokeAll(CMSDatabase db, int userId) =>
            db.Database.ExecuteSqlCommand("DELETE FROM ConnectedUsers WHERE UserID={0}", userId);
    }
}
