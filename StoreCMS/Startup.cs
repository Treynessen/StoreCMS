using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Trane.Database.Context;
using Trane.Middlwares;
using Trane.DependencyInjection;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using System.Text;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddMvc();

        services.AddTransient(provider =>
        {
            IHostingEnvironment env = provider.GetRequiredService<IHostingEnvironment>();
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("AppConfigurations/core_config.json");
            return new AccessLevelConfiguration(builder.Build().GetSection("AccessLevelSettings"));
        });

        services.AddDbContext<CMSDatabase>(options =>
        {
            var builder = new ConfigurationBuilder()
            .SetBasePath(services.BuildServiceProvider().GetRequiredService<IHostingEnvironment>().ContentRootPath)
            .AddJsonFile("AppConfigurations/core_config.json");
            options.UseSqlServer(builder.Build().GetConnectionString("DefaultConnection"));
        });

        services.AddSingleton<Translit>();

        services.AddTransient<ILoginFormLocalization>(provider => new RuLoginFormLocalization());
        services.AddTransient<IAdminPanelPageLocalization>(provider => new RuAdminPanelPageLocalization());
        services.AddTransient<IPageLocalization>(provider => new RuPageLocalization());
    }

    public void Configure(IApplicationBuilder app, IHostingEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        app.UseStaticFiles();
        app.UseAccessLevelInItem("admin");
        app.UseMvc(routeBuilder =>
        {
            routeBuilder.MapRoute(
                name: "admin_panel",
                template: "~/admin",
                defaults: new { controller = "AdminPanel", action = "AdminPanel" }
            );
        });
    }
}