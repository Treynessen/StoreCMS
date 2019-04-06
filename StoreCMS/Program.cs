using System;
using System.Linq;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Trane.Database.Context;
using Trane.Database.Entities;

public class Program
{
    private static void SetDefaultUser(CMSDatabase db)
    {
        if (db.Users.CountAsync().Result == 0)
        {
            UserType userType = db.UserTypes.FirstOrDefault(t => t.Name.Equals("Admin", StringComparison.CurrentCultureIgnoreCase));
            if (userType == null) userType = db.UserTypes.First();
            db.Users.Add(new User { ID = 1, Login = "admin", Password = "admin", UserType = userType });
            db.SaveChanges();
        }
    }

    public static void Main(string[] args)
    {

        IWebHost host = WebHost.CreateDefaultBuilder(args)
            .UseStartup<Startup>()
            .UseWebRoot("Storage")
            .Build();
        using (var scope = host.Services.CreateScope())
        {
            SetDefaultUser(scope.ServiceProvider.GetRequiredService<CMSDatabase>());
        }
        host.Run();
    }
}
