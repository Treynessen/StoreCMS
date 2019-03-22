using Trane.Localizations;
using Trane.Db.Context;
using Trane.Functions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class Startup
{
    private IConfiguration coreConfiguration;

    public Startup(IHostingEnvironment env)
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(env.ContentRootPath)
            .AddJsonFile("Configurations/core_config.json");
        coreConfiguration = builder.Build();
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddMvc();
        services.AddTransient<ILoginFormLocalization>(provider => new RuLoginFormLocalization());
        services.AddDbContext<CMSContext>(options => options
            .UseSqlServer(coreConfiguration.GetConnectionString("DefaultConnection")));
    }

    public void Configure(IApplicationBuilder app, IHostingEnvironment env, CMSContext db)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        ActionsWithDb.SetDefaultUser(db);
        app.UseStaticFiles();
        app.UseMvc(routeBuilder =>
        {
            routeBuilder.MapRoute(
                name: "admin_panel",
                template: "~/admin",
                defaults: new { controller = "AdminPanel", action = "LoginForm" }
            );
        });
    }
}