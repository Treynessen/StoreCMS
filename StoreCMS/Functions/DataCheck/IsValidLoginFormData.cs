using System;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Treynessen.AdminPanelTypes;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;

namespace Treynessen.Functions
{
    public static partial class DataCheck
    {
        public static bool IsValidLoginFormData(CMSDatabase db, LoginFormModel data, HttpContext context)
        {
            User user = db.Users.FirstOrDefaultAsync(u => u.Login.Equals(data.Login, StringComparison.CurrentCulture)).Result;
            if (user == null) return false;
            if (!user.Password.Equals(data.Password, StringComparison.CurrentCulture)) return false;
            ActionsWithDatabase.AddConnectedUser(db, user, context);
            return true;
        }
    }
}