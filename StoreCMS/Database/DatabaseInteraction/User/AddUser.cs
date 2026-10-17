using System;
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
        public static void AddUser(CMSDatabase db, UserModel model, HttpContext context, out int statusCode)
            => AddUser(db, model, context, out statusCode, out _);

        public static void AddUser(CMSDatabase db, UserModel model, HttpContext context, out int statusCode, out string validationError)
        {
            validationError = null;
            if (model == null) validationError = "user_data_required";
            else if (string.IsNullOrEmpty(model.Login)) validationError = "user_login_required";
            else if (!CorrectLogin(model.Login)) validationError = "user_login_invalid";
            else if (string.IsNullOrEmpty(model.NewPassword)) validationError = "user_password_required";
            else if (!CorrectPassword(model.NewPassword)) validationError = "user_password_invalid";
            else if (!model.UserTypeId.HasValue) validationError = "user_role_required";
            else if (db.UserTypes.FirstOrDefault(ut => ut.ID == model.UserTypeId.Value) == null)
                validationError = "user_role_invalid";
            if (validationError != null)
            {
                statusCode = 422;
                return;
            }
            if (db.Users.AsNoTracking().FirstOrDefault(u => u.Login.Equals(model.Login, StringComparison.Ordinal)) != null)
            {
                validationError = "user_login_taken";
                statusCode = 409;
                return;
            }
            User user = new User
            {
                Login = model.Login,
                Password = SecurityFunctions.GetPasswordHash(model.NewPassword),
                IdleTime = 10,
                UserTypeID = model.UserTypeId.Value
            };
            try
            {
                db.Users.Add(user);
            }
            catch (DbUpdateException)
            {
                statusCode = 409;
                return;
            }
            db.SaveChanges();
            statusCode = 201;

            LogManagementFunctions.AddAdminPanelLog(
                db: db,
                context: context,
                info: $"{user.Login} (ID-{user.ID.ToString()}): {(context.Items["LogLocalization"] as IAdminPanelLogLocalization)?.UserAdded}"
            );
        }
    }
}