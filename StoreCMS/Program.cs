using System;
using System.Linq;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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
            SetDefaultUser(scope.ServiceProvider.GetRequiredService<CMSDatabase>());
        }
        host.Run();
    }

    private static void SetDefaultUser(CMSDatabase db)
    {
        if (db.Users.CountAsync().Result == 0)
        {
            UserType userType = db.UserTypes.FirstOrDefaultAsync(t => t.Name.Equals("Admin", StringComparison.CurrentCultureIgnoreCase)).Result;
            if (userType == null) userType = db.UserTypes.First();
            db.Users.Add(new User { ID = 1, Login = "admin", Password = "admin", UserType = userType });
            db.SaveChanges();
        }
    }
}
