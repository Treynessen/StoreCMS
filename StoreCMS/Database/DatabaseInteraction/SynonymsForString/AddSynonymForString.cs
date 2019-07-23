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
        public static void AddSynonymForString(CMSDatabase db, SynonymForStringModel model, out bool successfullyCompleted)
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
                synonymForString.ID = GetDatabaseRawID(db.SynonymsForStrings);
                db.SynonymsForStrings.Add(synonymForString);
                db.SaveChanges();
                successfullyCompleted = true;
            }
            else successfullyCompleted = false;
        }
    }
}