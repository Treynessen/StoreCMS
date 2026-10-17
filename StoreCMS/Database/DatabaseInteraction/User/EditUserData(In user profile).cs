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
            => EditUserData(db, model, context, out statusCode, out _);

        public static void EditUserData(CMSDatabase db, UserModel model, HttpContext context, out int statusCode, out string validationError)
        {
            validationError = null;
            if (model == null) validationError = "user_data_required";
            else if (!model.ID.HasValue) validationError = "user_id_required";
            else if (string.IsNullOrEmpty(model.Login)) validationError = "user_login_required";
            else if (!CorrectLogin(model.Login)) validationError = "user_login_invalid";
            else if (!string.IsNullOrEmpty(model.NewPassword) && !CorrectPassword(model.NewPassword))
                validationError = "user_password_invalid";
            else if (model.IdleTime < 10 || model.IdleTime > 10080) validationError = "user_idle_time_invalid";
            if (validationError != null)
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
                validationError = "user_login_taken";
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
