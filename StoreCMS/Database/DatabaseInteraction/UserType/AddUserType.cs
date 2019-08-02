using System;
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
        public static void AddUserType(CMSDatabase db, UserTypeModel model, HttpContext context, out bool successfullyCompleted)
        {
            if (string.IsNullOrEmpty(model.Name) || !model.AccessLevel.HasValue || !Enum.IsDefined(typeof(AccessLevel), model.AccessLevel.Value))
            {
                successfullyCompleted = false;
                return;
            }
            UserType userType = new UserType
            {
                Name = model.Name,
                AccessLevel = model.AccessLevel.Value
            };
            db.UserTypes.Add(userType);
            db.SaveChanges();
            successfullyCompleted = true;

            LogManagementFunctions.AddAdminPanelLog(
                db: db,
                context: context,
                info: $"{userType.Name} ({userType.AccessLevel.ToString()}): {(context.Items["LogLocalization"] as IAdminPanelLogLocalization)?.UserTypeAdded}"
            );
        }
    }
}