using System.Data;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;
using Treynessen.Security;

namespace Treynessen.Database
{
    public static partial class DatabaseInteraction
    {
        public static void Exit(CMSDatabase db, User user, HttpContext context, out int statusCode)
        {
            if (SessionToken.TryHash(context.Request.Cookies[SessionCookies.Name], out string hash))
            {
                var session = db.ConnectedUsers.AsNoTracking().SingleOrDefault(s => s.TokenHash == hash);
                if (session != null)
                    using (var transaction = db.Database.BeginTransaction(IsolationLevel.ReadCommitted))
                    {
                        SessionStore.LockUser(db, session.UserID);
                        SessionStore.Delete(db, hash);
                        transaction.Commit();
                    }
            }
            SessionCookies.ClearLegacy(context);
            statusCode = 200;
        }
    }
}
