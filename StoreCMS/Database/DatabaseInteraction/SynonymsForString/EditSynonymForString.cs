using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Treynessen.Localization;
using Treynessen.LogManagement;
using Treynessen.AdminPanelTypes;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;

namespace Treynessen.Database
{
    public static partial class DatabaseInteraction
    {
        public static void EditSynonymForString(CMSDatabase db, int? itemID, SynonymForStringModel model, HttpContext context, out bool successfullyCompleted)
        {
            if (itemID.HasValue && !string.IsNullOrEmpty(model.String) && !string.IsNullOrEmpty(model.Synonym))
            {
                SynonymForString match = db.SynonymsForStrings.AsNoTracking()
                    .FirstOrDefault(s => (s.String.Equals(model.String, StringComparison.InvariantCultureIgnoreCase) && s.Synonym.Equals(model.Synonym, StringComparison.InvariantCultureIgnoreCase))
                    || (s.String.Equals(model.Synonym, StringComparison.InvariantCultureIgnoreCase) && s.Synonym.Equals(model.String, StringComparison.InvariantCultureIgnoreCase)));
                if (match != null)
                {
                    successfullyCompleted = false;
                    return;
                }

                SynonymForString synonymForString = db.SynonymsForStrings.FirstOrDefault(s => s.ID == itemID);
                string oldString = synonymForString.String;
                string oldSynonym = synonymForString.Synonym;
                if (synonymForString == null 
                    || (oldString.Equals(model.String, StringComparison.InvariantCulture) && oldSynonym.Equals(model.Synonym, StringComparison.InvariantCulture)))
                {
                    successfullyCompleted = false;
                    return;
                }
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