using System;
using System.Data;
using Microsoft.EntityFrameworkCore;
using Treynessen.AdminPanelTypes;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;
using Treynessen.Security;

namespace Treynessen.Database
{
    public static partial class DatabaseInteraction
    {
        public static void ChangeInitialPassword(CMSDatabase db, User user, UserModel model, out int statusCode, Microsoft.AspNetCore.Http.HttpContext context = null)
        {
            if (user == null || !user.RequirePasswordChange)
            { statusCode = 403; return; }
            if (model == null || !CorrectPassword(model.NewPassword)
                || !string.Equals(model.NewPassword, model.ConfirmPassword, StringComparison.Ordinal))
            { statusCode = 422; return; }
            if (!SecurityFunctions.VerifyPassword(user, model.CurrentPassword))
            { statusCode = 403; return; }

            string newHash = SecurityFunctions.GetPasswordHash(model.NewPassword);
            try
            {
                using (var transaction = db.Database.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    var current = SessionStore.LockUser(db, user.ID);
                    if (current == null || current.Password != user.Password || !current.RequirePasswordChange)
                    { statusCode = 409; return; }
                    if (context != null && !SessionStore.IsCurrent(db, context, current, AdminSessionPolicy.Instance, DateTime.UtcNow))
                    { statusCode = 403; return; }
                    user.Password = newHash;
                    user.RequirePasswordChange = false;
                    db.SaveChanges();
                    SessionStore.RevokeAll(db, user.ID);
                    transaction.Commit();
                }
            }
            catch (DbUpdateConcurrencyException)
            { statusCode = 409; return; }
            statusCode = 200;
        }
    }
}
