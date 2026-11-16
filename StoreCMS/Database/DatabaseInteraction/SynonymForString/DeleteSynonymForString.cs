using System.Linq;
using Microsoft.AspNetCore.Http;
using Treynessen.Localization;
using Treynessen.LogManagement;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;

namespace Treynessen.Database
{
    public static partial class DatabaseInteraction
    {
        public static void DeleteSynonymForString(CMSDatabase db, int? itemID, HttpContext context, out bool successfullyCompleted)
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

                LogManagementFunctions.AddAdminPanelLog(
                    db: db,
                    context: context,
                    info: $"{synonymForString.String} -> {synonymForString.Synonym}: {(context.Items["LogLocalization"] as IAdminPanelLogLocalization)?.SynonymForStringDeleted}"
                );
            }
            else successfullyCompleted = false;
        }
    }
}