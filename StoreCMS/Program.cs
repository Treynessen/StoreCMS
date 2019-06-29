using System.Linq;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Treynessen.Security;
using Treynessen.Database;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;

public class Program
{
    public static void Main(string[] args)
    {
        IWebHost host = WebHost.CreateDefaultBuilder(args)
            .UseStartup<Startup>()
            .UseWebRoot("Storage")
            .Build();
        using (var scope = host.Services.CreateScope())
        {
            CMSDatabase db = scope.ServiceProvider.GetRequiredService<CMSDatabase>();
            SetDeafaultUserType(db);
            SetDeafaultUser(db);
        }
        host.Run();
    }

    private static void SetDeafaultUserType(CMSDatabase db)
    {
        UserType userType = db.UserTypes.AsNoTracking().FirstOrDefaultAsync(ut => ut.AccessLevel == AccessLevel.VeryHigh).Result;
        if (userType == null)
        {
            userType = new UserType
            {
                ID = DatabaseInteraction.GetDatabaseRawID(db.UserTypes),
                Name = "Admin",
                AccessLevel = AccessLevel.VeryHigh
            };
            db.UserTypes.Add(userType);
            db.SaveChanges();
            db.Entry(userType).State = EntityState.Detached;
        }
    }

    private static void SetDeafaultUser(CMSDatabase db)
    {
        bool createUser = false;
        if (db.Users.Count() == 0)
            createUser = true;
        else
        {
            createUser = true;
            foreach(var user in db.Users.ToArray())
            {
                db.Entry(user).Reference(u => u.UserType).Load();
                db.Entry(user).State = EntityState.Detached;
                db.Entry(user.UserType).State = EntityState.Detached;
                if(user.UserType.AccessLevel == AccessLevel.VeryHigh)
                {
                    createUser = false;
                    break;
                }
            }
        }
        if (createUser)
        {
            UserType userType = db.UserTypes.FirstOrDefaultAsync(ut => ut.AccessLevel == AccessLevel.VeryHigh).Result;
            User user = new User
            {
                ID = DatabaseInteraction.GetDatabaseRawID(db.Users),
                Login = "admin",
                Password = "admin",
                UserType = userType
            };
            db.Users.Add(user);
            db.SaveChanges();
            db.Entry(user).State = EntityState.Detached;
            db.Entry(userType).State = EntityState.Detached;
        }
    }
}