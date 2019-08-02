using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Treynessen.Security;
using Treynessen.Localization;
using Treynessen.LogManagement;
using Treynessen.AdminPanelTypes;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;

namespace Treynessen.Database
{
    public static partial class DatabaseInteraction
    {
        public static void EditUserType(CMSDatabase db, int? itemID, UserTypeModel model, HttpContext context, out bool successfullyCompleted)
        {
            if (string.IsNullOrEmpty(model.Name) || !model.AccessLevel.HasValue || !itemID.HasValue || !Enum.IsDefined(typeof(AccessLevel), model.AccessLevel.Value))
            {
                successfullyCompleted = false;
                return;
            }
            UserType userType = db.UserTypes.FirstOrDefault(ut => ut.ID == itemID.Value);
            if (userType == null || (userType.ID == 1 && model.AccessLevel.Value != userType.AccessLevel))
            {
                successfullyCompleted = false;
                return;
            }
            string oldName = userType.Name;
            AccessLevel oldAccessLevel = userType.AccessLevel;
            userType.Name = model.Name;
            userType.AccessLevel = model.AccessLevel.Value;
            db.SaveChanges();
            successfullyCompleted = true;

            LogManagementFunctions.AddAdminPanelLog(
                db: db,
                context: context,
                info: $"{oldName} ({oldAccessLevel.ToString()}) -> {userType.Name} ({userType.AccessLevel.ToString()}): {(context.Items["LogLocalization"] as IAdminPanelLogLocalization)?.UserTypeEdited}"
            );
        }
    }
}