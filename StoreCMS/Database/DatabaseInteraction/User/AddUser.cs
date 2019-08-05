using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
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
        {
            if (!CorrectLogin(model.Login) || !CorrectPassword(model.NewPassword) || !model.UserTypeId.HasValue 
                || db.UserTypes.FirstOrDefault(ut => ut.ID == model.UserTypeId.Value) == null)
            {
                statusCode = 422;
                return;
            }
            if (db.Users.AsNoTracking().FirstOrDefault(u => u.Login.Equals(model.Login, StringComparison.Ordinal)) != null)
            {
                statusCode = 409;
                return;
            }
            User user = new User
            {
                Login = model.Login,
                Password = model.NewPassword,
                IdleTime = 10,
                UserTypeID = model.UserTypeId.Value
            };
            db.Users.Add(user);
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