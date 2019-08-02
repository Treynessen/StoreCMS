using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Treynessen.AdminPanelTypes;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;

namespace Treynessen.Database
{
    public static partial class DatabaseInteraction
    {
        public static void EditUser(CMSDatabase db, int? itemID, UserModel model, HttpContext context, out bool successfullyCompleted)
        {
            if (!itemID.HasValue
                || string.IsNullOrEmpty(model.CurrentPassword)
                || string.IsNullOrEmpty(model.Login)
                || (!string.IsNullOrEmpty(model.NewPassword) && (model.NewPassword.Length < 5 || !CorrectPassword(model.NewPassword)))
                || !CorrectLogin(model.Login)
            )
            {
                successfullyCompleted = false;
                return;
            }
            User editableUser = db.Users.FirstOrDefault(u => u.ID == itemID.Value);
            if (editableUser == null 
                || editableUser != context.Items["User"] as User 
                || !editableUser.Password.Equals(model.CurrentPassword)
                || (!editableUser.Login.Equals(model.Login, StringComparison.Ordinal) && db.Users.FirstOrDefault(u => u.Login.Equals(model.Login, StringComparison.Ordinal)) != null)
            )
            {
                successfullyCompleted = false;
                return;
            }
            editableUser.Login = model.Login;
            if (!string.IsNullOrEmpty(model.NewPassword))
                editableUser.Password = model.NewPassword;
            editableUser.Email = model.Email;
            db.SaveChanges();
            successfullyCompleted = true;
        }
    }
}