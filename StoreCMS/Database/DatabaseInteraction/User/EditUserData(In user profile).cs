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
        public static void EditUserData(CMSDatabase db, UserModel model, HttpContext context, out int statusCode)
        {
            if (!model.ID.HasValue
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
                info: $"{(context.Items["LogLocalization"] as IAdminPanelLogLocalization)?.UserDataEdited}"
            );
        }
    }
}