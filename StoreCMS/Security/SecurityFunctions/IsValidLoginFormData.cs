using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Treynessen.Database;
using Treynessen.Localization;
using Treynessen.LogManagement;
using Treynessen.AdminPanelTypes;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;

namespace Treynessen.Security
{
    public static partial class SecurityFunctions
    {
        public static bool IsValidLoginFormData(CMSDatabase db, LoginFormModel data, HttpContext context)
        {
            if (string.IsNullOrEmpty(data.Login) || string.IsNullOrEmpty(data.Password)) return false;
            User user = db.Users.FirstOrDefault(u => u.Login.Equals(data.Login, StringComparison.InvariantCulture));
            if (user == null) return false;
            if (!user.Password.Equals(data.Password, StringComparison.InvariantCulture)) return false;
            DatabaseInteraction.AddConnectedUser(db, user, context);
            context.Items["User"] = user;
            LogManagementFunctions.AddAdminPanelLog(
                db: db,
                context: context,
                info: (context.Items["LogLocalization"] as IAdminPanelLogLocalization)?.LoggedIn
            );
            return true;
        }
    }
}