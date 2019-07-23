using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Treynessen.AdminPanelTypes;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;

namespace Treynessen.Database
{
    public static partial class DatabaseInteraction
    {
        public static void EditSynonymForString(CMSDatabase db, int? itemID, SynonymForStringModel model, out bool successfullyCompleted)
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
                if (synonymForString == null)
                {
                    successfullyCompleted = false;
                    return;
                }
                synonymForString.String = model.String;
                synonymForString.Synonym = model.Synonym;
                db.SaveChanges();
                successfullyCompleted = true;
            }
            else successfullyCompleted = false;
        }
    }
}