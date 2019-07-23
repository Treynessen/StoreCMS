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
        public static void DeleteSynonymForString(CMSDatabase db, int? itemID, out bool successfullyCompleted)
        {
            if (itemID.HasValue)
            {
                SynonymForString synonymForString = db.SynonymsForStrings.FirstOrDefault(s => s.ID == itemID);
                if (synonymForString == null)
                {
                    successfullyCompleted = false;
                    return;
                }
                db.Remove(synonymForString);
                db.SaveChanges();
                successfullyCompleted = true;
            }
            else successfullyCompleted = false;
        }
    }
}