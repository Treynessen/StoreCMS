using Trane.Db.Context;
using Trane.Db.Entities;
using System.Linq;

namespace Trane.Functions
{
    public static class ActionsWithDb
    {
        public static void SetDefaultUser(CMSContext db)
        {
            if (db.Users.Count() == 0)
            {
                UserType userType = db.UserTypes.FirstOrDefault(t => t.Name == "Admin");
                if (userType == null) userType = db.UserTypes.First();
                db.Users.Add(new User { Login = "admin", Password = "admin", UserType = userType });
                db.SaveChanges();
            }
        }
    }
}
