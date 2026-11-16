using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Treynessen.Localization;
using Treynessen.LogManagement;
using Treynessen.AdminPanelTypes;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;
using Treynessen.Database.Queries;

namespace Treynessen.Database
{
    public static partial class DatabaseInteraction
    {
        public static void EditSynonymForString(CMSDatabase db, int? itemID, SynonymForStringModel model, HttpContext context, out bool successfullyCompleted)
        {
            if (itemID.HasValue && !string.IsNullOrEmpty(model.String) && !string.IsNullOrEmpty(model.Synonym)
                && ProductSearch.Fold(db, model.String) != ProductSearch.Fold(db, model.Synonym))
            {
                SynonymForString match = ProductSearch.MatchingPair(db, model.String, model.Synonym).AsNoTracking()
                    .FirstOrDefault(s => s.ID != itemID.Value);
                if (match != null)
                {
                    successfullyCompleted = false;
                    return;
                }

                SynonymForString synonymForString = db.SynonymsForStrings.FirstOrDefault(s => s.ID == itemID);
                if (synonymForString == null)
                {
                    successfullyCompleted = false;
                    return;
                }
                string oldString = synonymForString.String;
                string oldSynonym = synonymForString.Synonym;
                synonymForString.String = model.String;
                synonymForString.Synonym = model.Synonym;
                db.SaveChanges();
                successfullyCompleted = true;

                LogManagementFunctions.AddAdminPanelLog(
                    db: db,
                    context: context,
                    info: $"{synonymForString.String} -> {synonymForString.Synonym}: " +
                    $"{(context.Items["LogLocalization"] as IAdminPanelLogLocalization)?.SynonymForStringEditedTo} {oldString} -> {oldSynonym}"
                );
            }
            else successfullyCompleted = false;
        }
    }
}
