using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Treynessen.AdminPanelTypes;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;

namespace Treynessen.Functions
{
    public static partial class DataCheck
    {
        public static bool IsValidLoginFormData(CMSDatabase db, LoginFormModel data, HttpContext context)
        {
            User user = db.Users.FirstOrDefault(u => u.Login.Equals(data.Login, StringComparison.CurrentCulture));
            if (user == null) return false;
            if (!user.Password.Equals(data.Password, StringComparison.CurrentCulture)) return false;
            ActionsWithDatabase.AddConnectedUser(db, user, context);
            return true;
        }
    }
}