﻿using System.Linq;
using System.Globalization;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Treynessen.Security;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;
using Treynessen.SettingsManagement;

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
            ConfigurationHandler config = scope.ServiceProvider.GetRequiredService<ConfigurationHandler>();
            try
            {
                CultureInfo.CurrentCulture = new CultureInfo(config.GetConfigValue("LanguageSettings:CurrentLanguage"));
            }
            catch { }
            CMSDatabase db = scope.ServiceProvider.GetRequiredService<CMSDatabase>();
            SetDeafaultUserType(db);
            SetDeafaultUser(db);
        }
        host.Run();
    }

    private static void SetDeafaultUserType(CMSDatabase db)
    {
        UserType userType = db.UserTypes.AsNoTracking().FirstOrDefault(ut => ut.AccessLevel == AccessLevel.VeryHigh);
        if (userType == null)
        {
            db.UserTypes.Add(new UserType { Name = "Admin", AccessLevel = AccessLevel.VeryHigh });
            db.SaveChanges();
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
            foreach (var user in db.Users.ToArray())
            {
                user.UserType = db.UserTypes.FirstOrDefault(ut => ut.ID == user.UserTypeID);
                if (user.UserType.AccessLevel == AccessLevel.VeryHigh)
                {
                    createUser = false;
                    break;
                }
            }
        }
        if (createUser)
        {
            UserType userType = db.UserTypes.FirstOrDefault(ut => ut.AccessLevel == AccessLevel.VeryHigh);
            User user = new User
            {
                Login = "admin",
                Password = SecurityFunctions.GetPasswordHash("admin"),
                UserType = userType,
                IdleTime = 10
            };
            db.Users.Add(user);
            db.SaveChanges();
        }
    }
}