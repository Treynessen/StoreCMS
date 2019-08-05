using System.Linq;
using Microsoft.AspNetCore.Http;
using Treynessen.Localization;
using Treynessen.LogManagement;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;

namespace Treynessen.Database
{
    public static partial class DatabaseInteraction
    {
        public static void DeleteUserType(CMSDatabase db, int? itemID, HttpContext context, out bool successfullyCompleted)
        {
            if (!itemID.HasValue || itemID.Value == 1)
            {
                successfullyCompleted = false;
                return;
            }
            UserType userType = db.UserTypes.FirstOrDefault(ut => ut.ID == itemID.Value);
            if (userType == null)
            {
                successfullyCompleted = false;
                return;
            }
            if (userType == (context.Items["User"] as User).UserType)
            {
                context.Items["User"] = null;
            }
            db.UserTypes.Remove(userType);
            db.SaveChanges();
            successfullyCompleted = true;

            LogManagementFunctions.AddAdminPanelLog(
                db: db,
                context: context,
                info: $"{userType.Name} ({userType.AccessLevel.ToString()}): {(context.Items["LogLocalization"] as IAdminPanelLogLocalization)?.UserTypeDeleted}"
            );
        }
    }
}