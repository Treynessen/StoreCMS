using System;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Treynessen.AdminPanelTypes;
using Treynessen.Database;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;

namespace Treynessen.Security
{
    public static partial class SecurityFunctions
    {
        public static bool IsValidLoginFormData(CMSDatabase db, LoginFormModel data, HttpContext context)
        {
            if (string.IsNullOrEmpty(data.Login) || string.IsNullOrEmpty(data.Password)) return false;
            User user = db.Users.FirstOrDefaultAsync(u => u.Login.Equals(data.Login, StringComparison.InvariantCulture)).Result;
            if (user == null) return false;
            if (!user.Password.Equals(data.Password, StringComparison.InvariantCulture)) return false;
            DatabaseInteraction.AddConnectedUser(db, user, context);
            return true;
        }
    }
}