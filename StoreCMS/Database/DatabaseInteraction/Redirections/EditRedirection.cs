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
            if (string.IsNullOrEmpty(model.RedirectionPath) || string.IsNullOrEmpty(model.RequestPath) || !itemID.HasValue)
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
            model.RequestPath = model.RequestPath.ToLower().Replace('\\', '/');
            if (!model.RequestPath[0].Equals('/'))
                model.RequestPath = model.RequestPath.Insert(0, "/");
            if (model.RequestPath.Length > 1 && model.RequestPath[model.RequestPath.Length - 1].Equals('/'))
                model.RequestPath = model.RequestPath.Remove(model.RequestPath.Length - 1, 1);
            model.RedirectionPath = model.RedirectionPath.ToLower().Replace('\\', '/');
            if (!model.RedirectionPath[0].Equals('/'))
                model.RedirectionPath = model.RedirectionPath.Insert(0, "/");
            if (model.RedirectionPath.Length > 1 && model.RedirectionPath[model.RedirectionPath.Length - 1].Equals('/'))
                model.RedirectionPath = model.RedirectionPath.Remove(model.RedirectionPath.Length - 1, 1);
            if (model.RequestPath.Equals(model.RedirectionPath, StringComparison.InvariantCulture) ||
                (oldRequestPath.Equals(model.RequestPath, StringComparison.InvariantCulture) && oldRedirectionPath.Equals(model.RedirectionPath, StringComparison.InvariantCulture)))
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