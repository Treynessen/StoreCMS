using Trane.Middleware;
using Trane.Localizations;
using Trane.Db.Context;
using Trane.Functions;
using Trane.RouteConstraints;
using Trane.Configurations;
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
    private string defaultConnection;

    public Startup(IHostingEnvironment env)
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(env.ContentRootPath)
            .AddJsonFile("Configurations/core_config.json");
        defaultConnection = builder.Build().GetConnectionString("DefaultConnection");
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddMvc();

        services.AddTransient(provider =>
        {
            IHostingEnvironment env = provider.GetRequiredService<IHostingEnvironment>();
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("Configurations/core_config.json");
            return new ClearanceLevelConfiguration(builder.Build().GetSection("ClearanceLevelSettings"));
        });

        services.AddTransient<ILoginFormLocalization>(provider => new RuLoginFormLocalization());
        services.AddTransient<IAdminPanelPageLocalization>(provider => new RuAdminPanelPageLocalization());
        services.AddTransient<IPageLocalization>(provider => new RuPageLocalization());

        services.AddDbContext<CMSContext>(options => options.UseSqlServer(defaultConnection));
    }

    public void Configure(IApplicationBuilder app, IHostingEnvironment env, CMSContext db)
    {
        //app.Run(async context =>
        //{
        //    var conf = context.RequestServices.GetRequiredService<ClearanceLevelConfiguration>();
        //    await context.Response.WriteAsync(conf.ShowCategories.ToString());
        //});

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
        app.UseClearanceLevelMiddleware("admin");
        app.UseStaticFiles();
        app.UseMvc(routeBuilder =>
        {
            routeBuilder.MapRoute(
                name: "admin_panel",
                template: "~/admin",
                defaults: new { controller = "AdminPanel", action = "AdminPanel" }
            );
            //routeBuilder.MapRoute(
            //    name: "simple_page",
            //    template: "/{*breadcrumbs}",
            //    defaults: new { controller = "Page", action = "PageHandler" },
            //    constraints: new { breadcrumbs = new SimplePageConstraint(db) }
            //);
        });

        app.Run(async context =>
        {
            var _db = context.RequestServices.GetRequiredService<CMSContext>();
            StringBuilder builder = new StringBuilder();
            var c_users = _db.ConnectedUsers.ToListAsync().Result;
            foreach (var u in c_users)
                builder.Append($"{u.UserName}<br>{u.LoginKey}<br>{u.UserAgent}<br>{u.LastActionTime}<br><br>");
            context.Response.ContentType = "text/html;charset=utf-8";
            await context.Response.WriteAsync(builder.ToString());
        });
    }
}