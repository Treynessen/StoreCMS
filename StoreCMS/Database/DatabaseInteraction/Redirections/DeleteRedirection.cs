using System.Linq;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;

namespace Treynessen.Database
{
    public static partial class DatabaseInteraction
    {
        public static void DeleteRedirection(CMSDatabase db, int? itemID, out bool successfullyCompleted)
        {
            if (itemID.HasValue)
            {
                Redirection redirection = db.Redirections.FirstOrDefault(r => r.ID == itemID);
                if (redirection == null)
                {
                    successfullyCompleted = false;
                    return;
                }
                db.Remove(redirection);
                db.SaveChanges();
                successfullyCompleted = true;
            }
            else successfullyCompleted = false;
        }
    }
}