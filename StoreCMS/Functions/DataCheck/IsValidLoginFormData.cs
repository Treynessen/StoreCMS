using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Trane.Database.Context;
using Trane.Database.Entities;
using Trane.OtherTypes;

namespace Trane.Functions
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
