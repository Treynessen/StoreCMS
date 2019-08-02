using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
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
            if (!user.Password.Equals(data.Password, StringComparison.Ordinal)) return false;
            return true;
        }
    }
}