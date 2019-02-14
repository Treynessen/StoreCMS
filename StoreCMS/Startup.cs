using Trane.Localization;
using Trane.Configurations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc;

public class Startup
{
    private ConfigurationsContainer configurations;

    public Startup(IHostingEnvironment env)
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(env.ContentRootPath)
            .AddIniFile("Core/Config/paths_config.ini");
        IConfiguration pathsConfig = builder.Build();
        builder.AddIniFile("Core/Config/core_config.ini");
        IConfiguration coreConfig = builder.Build();
        configurations = new ConfigurationsContainer(coreConfig, pathsConfig);
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddMvc();
        services.AddSingleton<ILocalization>(provider =>
        {
            // Получение информации из конфигурации
            // И возврат соответствующего объекта
            return new RuLocalization();
        });
        services.AddSingleton(provider => configurations);
    }

    public void Configure(IApplicationBuilder app, IHostingEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        app.UseStaticFiles();
        app.UseMvc(routeBuilder =>
        {
            routeBuilder.MapRoute(
                name: "adminPanel_Route",
                template: "~/admin",
                defaults: new { area = "AdminPanel", controller = "Admin", action = "LoginForm" }
            );
            routeBuilder.MapRoute(
                name: "404",
                template: "~/{*directories}",
                defaults: new { area = "page404", controller = "Error", action = "Index" }
            );
        });
    }
}
