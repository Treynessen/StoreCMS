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
        public static void EditUser(CMSDatabase db, int? userID, UserModel model, HttpContext context, out int statusCode)
            => EditUser(db, userID, model, context, out statusCode, out _);

        public static void EditUser(CMSDatabase db, int? userID, UserModel model, HttpContext context, out int statusCode, out string validationError)
        {
            validationError = null;
            if (!userID.HasValue) validationError = "user_id_required";
            else if (model == null) validationError = "user_data_required";
            else if (!model.UserTypeId.HasValue) validationError = "user_role_required";
            else if (db.UserTypes.AsNoTracking().FirstOrDefault(ut => ut.ID == model.UserTypeId.Value) == null)
                validationError = "user_role_invalid";
            if (validationError != null)
            {
                statusCode = 422;
                return;
            }
            if (userID.Value == 1)
            {
                statusCode = 200;
                return;
            }
            User editableUser = db.Users.FirstOrDefault(u => u.ID == userID.Value);
            if (editableUser == null)
            {
                statusCode = 404;
                return;
            }
            editableUser.UserTypeID = model.UserTypeId.Value;
            db.SaveChanges();
            statusCode = 200;

            LogManagementFunctions.AddAdminPanelLog(
                db: db,
                context: context,
                info: $"{editableUser.Login} (ID-{editableUser.ID.ToString()}): {(context.Items["LogLocalization"] as IAdminPanelLogLocalization)?.UserEdited}"
            );
        }
    }
}