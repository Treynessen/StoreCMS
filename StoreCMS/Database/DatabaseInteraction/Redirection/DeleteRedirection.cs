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
        public static void DeleteRedirection(CMSDatabase db, int? itemID, HttpContext context, out bool successfullyCompleted)
        {
            if (itemID.HasValue)
            {
                Redirection redirection = db.Redirections.FirstOrDefault(r => r.ID == itemID);
                if (redirection == null)
                {
                    successfullyCompleted = false;
                    return;
                }
                db.Remove(redirection);
                db.SaveChanges();
                successfullyCompleted = true;

                LogManagementFunctions.AddAdminPanelLog(
                    db: db,
                    context: context,
                    info: $"{redirection.RequestPath} -> {redirection.RedirectionPath}: {(context.Items["LogLocalization"] as IAdminPanelLogLocalization)?.RedirectionDeleted}"
                );
            }
            else successfullyCompleted = false;
        }
    }
}