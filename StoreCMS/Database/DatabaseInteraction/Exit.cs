using System.Linq;
using Microsoft.AspNetCore.Http;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;

namespace Treynessen.Database
{
    public static partial class DatabaseInteraction
    {
        public static void Exit(CMSDatabase db, User user, HttpContext context, out int statusCode)
        {
            if (user == null)
            {
                statusCode = 404;
                return;
            }
            ConnectedUser connectedUser = db.ConnectedUsers.FirstOrDefault(cu => cu.UserID == user.ID);
            if (connectedUser == null)
            {
                statusCode = 404;
                return;
            }
            db.ConnectedUsers.Remove(connectedUser);
            db.SaveChanges();
            statusCode = 200;
        }
    }
}