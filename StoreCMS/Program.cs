using System.Linq;
using System.Globalization;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Treynessen.Security;
using Treynessen.Database.Context;
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
            MySqlSecurityInstallation.Initialize(db);
        }
        host.Run();
    }

}
