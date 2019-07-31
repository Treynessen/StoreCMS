using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Treynessen.Functions;
using Treynessen.Localization;
using Treynessen.LogManagement;
using Treynessen.AdminPanelTypes;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;

namespace Treynessen.Database
{
    public static partial class DatabaseInteraction
    {
        public static void EditRedirection(CMSDatabase db, int? itemID, RedirectionModel model, HttpContext context, out bool successfullyCompleted)
        {
            if (string.IsNullOrEmpty(model.RedirectionPath) || string.IsNullOrEmpty(model.RequestPath) || !itemID.HasValue
                || model.RequestPath.Equals(model.RedirectionPath, StringComparison.OrdinalIgnoreCase))
            {
                successfullyCompleted = false;
                return;
            }
            Redirection redirection = db.Redirections.FirstOrDefault(r => r.ID == itemID.Value);
            if (redirection == null)
            {
                successfullyCompleted = false;
                return;
            }
            string oldRequestPath = redirection.RequestPath;
            string oldRedirectionPath = redirection.RedirectionPath;
            model.RequestPath = GetCorrectUrlPath(model.RequestPath);
            model.RedirectionPath = GetCorrectUrlPath(model.RedirectionPath);
            if (model.RequestPath.Equals(model.RedirectionPath, StringComparison.Ordinal))
            {
                successfullyCompleted = false;
                return;
            }
            redirection.RequestPath = model.RequestPath;
            redirection.RequestPathHash = OtherFunctions.GetHashFromString(model.RequestPath);
            redirection.RedirectionPath = model.RedirectionPath;
            db.SaveChanges();
            successfullyCompleted = true;

            LogManagementFunctions.AddAdminPanelLog(
                db: db,
                context: context,
                info: $"{oldRequestPath} -> {oldRedirectionPath}: {(context.Items["LogLocalization"] as IAdminPanelLogLocalization)?.RedirectionEditedTo} {redirection.RequestPath} -> {redirection.RedirectionPath}"
            );
        }
    }
}