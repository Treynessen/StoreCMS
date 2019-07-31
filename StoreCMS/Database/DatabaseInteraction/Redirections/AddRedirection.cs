using System;
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
        public static void AddRedirection(CMSDatabase db, RedirectionModel model, HttpContext context,  out bool successfullyCompleted)
        {
            if (string.IsNullOrEmpty(model.RedirectionPath) || string.IsNullOrEmpty(model.RequestPath) 
                || model.RequestPath.Equals(model.RedirectionPath, StringComparison.OrdinalIgnoreCase))
            {
                successfullyCompleted = false;
                return;
            }
            model.RequestPath = GetCorrectUrlPath(model.RequestPath);
            model.RedirectionPath = GetCorrectUrlPath(model.RedirectionPath);
            if (model.RequestPath.Equals(model.RedirectionPath, StringComparison.Ordinal))
            {
                successfullyCompleted = false;
                return;
            }
            Redirection redirection = new Redirection
            {
                RequestPath = model.RequestPath,
                RequestPathHash = OtherFunctions.GetHashFromString(model.RequestPath),
                RedirectionPath = model.RedirectionPath
            };
            db.Redirections.Add(redirection);
            db.SaveChanges();
            successfullyCompleted = true;

            LogManagementFunctions.AddAdminPanelLog(
                db: db,
                context: context,
                info: $"{redirection.RequestPath} -> {redirection.RedirectionPath}: {(context.Items["LogLocalization"] as IAdminPanelLogLocalization)?.RedirectionAdded}"
            );
        }
    }
}