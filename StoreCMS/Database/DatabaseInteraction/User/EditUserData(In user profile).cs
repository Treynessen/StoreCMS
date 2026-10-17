using System;
using System.Data;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Treynessen.Security;
using Treynessen.Localization;
using Treynessen.LogManagement;
using Treynessen.AdminPanelTypes;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;

namespace Treynessen.Database
{
    public static partial class DatabaseInteraction
    {
        public static void EditUserData(CMSDatabase db, UserModel model, HttpContext context, out int statusCode)
        {
            if (model == null || !model.ID.HasValue
                || !CorrectLogin(model.Login)
                || (!string.IsNullOrEmpty(model.NewPassword) && !CorrectPassword(model.NewPassword))
                || model.IdleTime < 10 || model.IdleTime > 10080
            )
            {
                statusCode = 422;
                return;
            }
            User editableUser = db.Users.FirstOrDefault(u => u.ID == model.ID.Value);
            if (editableUser == null)
            {
                statusCode = 404;
                return;
            }
            else if (editableUser.ID != (context.Items["User"] as User)?.ID || !SecurityFunctions.VerifyPassword(editableUser, model.CurrentPassword))
            {
                statusCode = 403;
                return;
            }
            else if (!editableUser.Login.Equals(model.Login, StringComparison.Ordinal) && db.Users.FirstOrDefault(u => u.Login.Equals(model.Login, StringComparison.Ordinal)) != null)
            {
                statusCode = 409;
                return;
            }
            bool changingPassword = !string.IsNullOrEmpty(model.NewPassword);
            string newHash = changingPassword ? SecurityFunctions.GetPasswordHash(model.NewPassword) : null;
            try
            {
                using (var transaction = db.Database.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    var current = SessionStore.LockUser(db, editableUser.ID);
                    if (current == null || current.Password != editableUser.Password)
                    { statusCode = 409; return; }
                    if (!SessionStore.IsCurrent(db, context, current, AdminSessionPolicy.Instance, DateTime.UtcNow))
                    { statusCode = 403; return; }
                    editableUser.Login = model.Login;
                    if (changingPassword) editableUser.Password = newHash;
                    editableUser.IdleTime = model.IdleTime;
                    editableUser.Email = model.Email;
                    db.SaveChanges();
                    if (changingPassword) SessionStore.RevokeAll(db, editableUser.ID);
                    transaction.Commit();
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                statusCode = 409;
                return;
            }
            statusCode = 200;
            if (changingPassword) SessionCookies.ClearLegacy(context);

            LogManagementFunctions.TryAddCommittedAdminPanelLog(
                db: db,
                context: context,
                info: $"{(context.Items["LogLocalization"] as IAdminPanelLogLocalization)?.UserDataEdited}"
            );
        }
    }
}
