using System.Linq;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;

namespace Treynessen.Database
{
    public static partial class DatabaseInteraction
    {
        public static void DeleteUserType(CMSDatabase db, int? itemID, out bool successfullyCompleted)
        {
            if (!itemID.HasValue || itemID.Value == 1)
            {
                successfullyCompleted = false;
                return;
            }
            UserType userType = db.UserTypes.FirstOrDefault(ut => ut.ID == itemID.Value);
            if (userType == null)
            {
                successfullyCompleted = false;
                return;
            }
            db.UserTypes.Remove(userType);
            db.SaveChanges();
            successfullyCompleted = true;
        }
    }
}