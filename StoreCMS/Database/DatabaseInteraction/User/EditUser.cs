using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Treynessen.Localization;
using Treynessen.LogManagement;
using Treynessen.AdminPanelTypes;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;

namespace Treynessen.Database
{
    public static partial class DatabaseInteraction
    {
        public static void EditUser(CMSDatabase db, int? itemID, UserModel model, HttpContext context, out int statusCode)
        {
            if (!itemID.HasValue
                || string.IsNullOrEmpty(model.CurrentPassword)
                || string.IsNullOrEmpty(model.Login)
                || (!string.IsNullOrEmpty(model.NewPassword) && (model.NewPassword.Length < 5 || !CorrectPassword(model.NewPassword)))
                || !CorrectLogin(model.Login)
                || model.IdleTime < 10 || model.IdleTime > 10080
            )
            {
                statusCode = 422;
                return;
            }
            User editableUser = db.Users.FirstOrDefault(u => u.ID == itemID.Value);
            if (editableUser == null)
            {
                statusCode = 404;
                return;
            }
            else if (editableUser != context.Items["User"] as User || !editableUser.Password.Equals(model.CurrentPassword))
            {
                statusCode = 403;
                return;
            }
            else if (!editableUser.Login.Equals(model.Login, StringComparison.Ordinal) && db.Users.FirstOrDefault(u => u.Login.Equals(model.Login, StringComparison.Ordinal)) != null)
            {
                statusCode = 409;
                return;
            }
            editableUser.Login = model.Login;
            if (!string.IsNullOrEmpty(model.NewPassword))
                editableUser.Password = model.NewPassword;
            editableUser.IdleTime = model.IdleTime;
            editableUser.Email = model.Email;
            db.SaveChanges();
            statusCode = 200;

            LogManagementFunctions.AddAdminPanelLog(
                db: db,
                context: context,
                info: $"{editableUser.Login} (ID-{editableUser.ID.ToString()}): {(context.Items["LogLocalization"] as IAdminPanelLogLocalization)?.UserDataEdited}"
            );
        }
    }
}