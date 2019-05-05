﻿using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Treynessen.Security;
using Treynessen.OtherTypes;
using Treynessen.Middlewares;
using Treynessen.Localization;
using Treynessen.AdminPanelTypes;
using Treynessen.Database.Context;
using Treynessen.RouteConstraints;

using System;
using Microsoft.AspNetCore.Http;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddDbContext<CMSDatabase>(options =>
        {
            var builder = new ConfigurationBuilder()
            .SetBasePath(services.BuildServiceProvider().GetRequiredService<IHostingEnvironment>().ContentRootPath)
            .AddJsonFile("Configurations/core_configuration.json");
            options.UseSqlServer(builder.Build().GetConnectionString("DefaultConnection"));
        });

        services.AddMvc();

        services.AddTransient(provider =>
        {
            string rootPath = provider.GetRequiredService<IHostingEnvironment>().ContentRootPath;
            return new AccessLevelConfiguration(rootPath, "Configurations/accessLevel_configuration.json");
        });

        services.AddSingleton<Translit>();

        services.AddTransient<ILoginFormLocalization>(provider => new RuLoginFormLocalization());
        services.AddTransient<IAdminPanelPageLocalization>(provider => new RuAdminPanelPageLocalization());
        services.AddTransient<IPagesLocalization>(provider => new RuPagesLocalization());
        services.AddTransient<IProductsLocalization>(provider => new RuProductsLocalization());
        services.AddTransient<ITemplatesLocalization>(provider => new RuTemplatesLocalization());
        services.AddTransient<IFilesPageLocalization>(provider => new RuFilesPageLocalization());
    }

    public void Configure(IApplicationBuilder app, IHostingEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        app.UseStaticFiles();
        app.AddAccessLevelConfigInItemWhen("/admin");
        app.UseMvc(routeBuilder =>
        {
            routeBuilder.MapRoute(
                name: "admin_panel",
                template: "~/admin",
                defaults: new { controller = "AdminPanel", action = "AdminPanel" }
            );
            routeBuilder.MapRoute(
                name: "some_page",
                template: "{*directory}",
                defaults: new { controller = "PagesHandler", action = "RequestHandler" },
                constraints: new { directory = new UrlConstraint() }
            );
        });
    }
}