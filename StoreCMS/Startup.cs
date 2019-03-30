using Trane.Localizations;
using Trane.Db.Context;
using Trane.Functions;
using Trane.RouteConstraints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using System.Text;

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
        services.AddTransient<IMainPageLocalization>(provider => new RuMainPageLocalization());
        services.AddDbContext<CMSContext>(options => options
            .UseSqlServer(coreConfiguration.GetConnectionString("DefaultConnection")));
    }

    public void Configure(IApplicationBuilder app, IHostingEnvironment env, CMSContext db)
    {
        //app.Run(async context =>
        //{
            
        //    await context.Response.WriteAsync(context.Request.Path);
        //});

        //app.Run(async context =>
        //{
        //    StringBuilder builder = new StringBuilder();
        //    //foreach (var kvp in context.Request.Headers)
        //    //    builder.Append($"header[{kvp.Key}] = {kvp.Value}\n");
        //    foreach (var cookie in context.Request.Cookies)
        //        builder.Append($"cookie[{cookie.Key}] = {cookie.Value}\n");
        //    await context.Response.WriteAsync(builder.ToString());
        //});
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
                defaults: new { controller = "AdminPanel", action = "AdminPanel" }
            );
            routeBuilder.MapRoute(
                name: "simple_page",
                template: "/{*breadcrumbs}",
                defaults: new { controller = "Page", action = "PageHandler" },
                constraints: new { breadcrumbs = new SimplePageConstraint(db) }
            );
        });
    }
}