using System;
using Microsoft.EntityFrameworkCore;
using Treynessen.AdminPanelTypes;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;
using Treynessen.Security;

namespace Treynessen.Database
{
    public static partial class DatabaseInteraction
    {
        public static void ChangeInitialPassword(CMSDatabase db, User user, UserModel model, out int statusCode)
        {
            if (user == null || !user.RequirePasswordChange)
            { statusCode = 403; return; }
            if (model == null || !CorrectPassword(model.NewPassword)
                || !string.Equals(model.NewPassword, model.ConfirmPassword, StringComparison.Ordinal))
            { statusCode = 422; return; }
            if (!SecurityFunctions.VerifyPassword(user, model.CurrentPassword))
            { statusCode = 403; return; }

            user.Password = SecurityFunctions.GetPasswordHash(model.NewPassword);
            user.RequirePasswordChange = false;
            try { db.SaveChanges(); }
            catch (DbUpdateConcurrencyException)
            { statusCode = 409; return; }
            statusCode = 200;
        }
    }
}
