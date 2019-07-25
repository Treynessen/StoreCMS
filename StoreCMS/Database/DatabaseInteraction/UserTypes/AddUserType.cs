using System;
using Treynessen.Security;
using Treynessen.AdminPanelTypes;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;

namespace Treynessen.Database
{
    public static partial class DatabaseInteraction
    {
        public static void AddUserType(CMSDatabase db, UserTypeModel model, out bool successfullyCompleted)
        {
            if (string.IsNullOrEmpty(model.Name) || !model.AccessLevel.HasValue || !Enum.IsDefined(typeof(AccessLevel), model.AccessLevel.Value))
            {
                successfullyCompleted = false;
                return;
            }
            UserType userType = new UserType
            {
                ID = GetDatabaseRawID(db.UserTypes),
                Name = model.Name,
                AccessLevel = model.AccessLevel.Value
            };
            db.UserTypes.Add(userType);
            db.SaveChanges();
            successfullyCompleted = true;
        }
    }
}