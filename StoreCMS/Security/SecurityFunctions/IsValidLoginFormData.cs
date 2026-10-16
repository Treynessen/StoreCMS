using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Treynessen.Localization;
using Treynessen.LogManagement;
using Treynessen.AdminPanelTypes;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;

namespace Treynessen.Security
{
    public static partial class SecurityFunctions
    {
        public static bool IsValidLoginFormData(CMSDatabase db, LoginFormModel data, HttpContext context, out User user)
        {
            if (string.IsNullOrEmpty(data.Login) || string.IsNullOrEmpty(data.Password))
            {
                user = null;
                return false;
            }
            user = db.Users.FirstOrDefault(u => u.Login.Equals(data.Login, StringComparison.Ordinal));
            if (user == null) return false;
            if (!VerifyPassword(user, data.Password))
            {
                LogManagementFunctions.AddAdminPanelLog(
                    db: db,
                    context: context,
                    info: $"{(context.Items["LogLocalization"] as IAdminPanelLogLocalization)?.FailedLoginAttempt}. IP: {context.Connection.RemoteIpAddress.ToString()}",
                    user: user
                );
                return false;
            }
            return true;
        }
    }
}
