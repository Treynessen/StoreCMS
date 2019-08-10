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
        public static void AddSynonymForString(CMSDatabase db, SynonymForStringModel model, HttpContext context, out bool successfullyCompleted)
        {
            if (!string.IsNullOrEmpty(model.String) && !string.IsNullOrEmpty(model.Synonym))
            {
                SynonymForString match = db.SynonymsForStrings.AsNoTracking()
                    .FirstOrDefault(s => (s.String.Equals(model.String, StringComparison.InvariantCultureIgnoreCase) && s.Synonym.Equals(model.Synonym, StringComparison.InvariantCultureIgnoreCase)) 
                    || (s.String.Equals(model.Synonym, StringComparison.InvariantCultureIgnoreCase) && s.Synonym.Equals(model.String, StringComparison.InvariantCultureIgnoreCase)));
                if (match != null)
                {
                    successfullyCompleted = false;
                    return;
                }

                SynonymForString synonymForString = new SynonymForString { String = model.String, Synonym = model.Synonym };
                db.SynonymsForStrings.Add(synonymForString);
                try
                {
                    db.SaveChanges();
                }
                catch (DbUpdateException)
                {
                    successfullyCompleted = false;
                    return;
                }
                successfullyCompleted = true;


                LogManagementFunctions.AddAdminPanelLog(
                    db: db,
                    context: context,
                    info: $"{synonymForString.String} -> {synonymForString.Synonym}: {(context.Items["LogLocalization"] as IAdminPanelLogLocalization)?.SynonymForStringAdded}"
                );
            }
            else successfullyCompleted = false;
        }
    }
}