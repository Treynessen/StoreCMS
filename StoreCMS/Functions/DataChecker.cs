using Trane.Db.Context;
using Trane.Db.Entities;
using Trane.ViewModels;
using System.Linq;

namespace Trane.Functions
{
    public static class DataChecker
    {
        public static bool CorrectUserData(CMSContext db, LoginFormData data)
        {
            User user = db.Users.FirstOrDefault(u => u.Login == data.Login);
            if (user == null) return false;
            if (user.Password != data.Password) return false;
            return true;
        }
    }
}
