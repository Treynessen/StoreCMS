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
        public static void DeleteUser(CMSDatabase db, int? userID, HttpContext context, out int statusCode)
        {
            if (!userID.HasValue || userID.Value == 1)
            {
                statusCode = 404;
                return;
            }
            User user = db.Users.FirstOrDefault(u => u.ID == userID.Value);
            if (user == null)
            {
                statusCode = 404;
                return;
            }
            db.Remove(user);
            db.SaveChanges();
            statusCode = 200;

            LogManagementFunctions.AddAdminPanelLog(
                db: db,
                context: context,
                info: $"{user.Login} (ID-{user.ID.ToString()}): {(context.Items["LogLocalization"] as IAdminPanelLogLocalization)?.UserDeleted}"
            );
        }
    }
}