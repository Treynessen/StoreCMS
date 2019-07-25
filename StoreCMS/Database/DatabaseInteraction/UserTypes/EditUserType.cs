using System;
using System.Linq;
using Treynessen.Security;
using Treynessen.AdminPanelTypes;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;

namespace Treynessen.Database
{
    public static partial class DatabaseInteraction
    {
        public static void EditUserType(CMSDatabase db, int? itemID, UserTypeModel model, out bool successfullyCompleted)
        {
            if (string.IsNullOrEmpty(model.Name) || !model.AccessLevel.HasValue || !itemID.HasValue || !Enum.IsDefined(typeof(AccessLevel), model.AccessLevel.Value))
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
            if (userType.ID == 1 && model.AccessLevel.Value != userType.AccessLevel)
            {
                successfullyCompleted = false;
                return;
            }
            userType.Name = model.Name;
            userType.AccessLevel = model.AccessLevel.Value;
            db.SaveChanges();
            successfullyCompleted = true;
        }
    }
}