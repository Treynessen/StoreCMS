using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Treynessen.AdminPanelTypes;
using Treynessen.PagesManagement;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;

namespace Treynessen.Database
{
    public static partial class DatabaseInteraction
    {
        public static void AddRedirection(CMSDatabase db, RedirectionModel model, out bool successfullyCompleted)
        {
            if (string.IsNullOrEmpty(model.RedirectionPath) || string.IsNullOrEmpty(model.RequestPath))
            {
                successfullyCompleted = false;
                return;
            }
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
            if (model.RequestPath.Equals(model.RedirectionPath, StringComparison.InvariantCulture))
            {
                successfullyCompleted = false;
                return;
            }
            db.Redirections.Add(new Redirection
            {
                ID = GetDatabaseRawID(db.Redirections),
                RequestPath = model.RequestPath,
                RequestPathHash = PagesManagementFunctions.GetHashFromRequestPath(model.RequestPath),
                RedirectionPath = model.RedirectionPath
            });
            db.SaveChanges();
            successfullyCompleted = true;
        }
    }
}