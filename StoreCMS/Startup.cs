using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Treynessen.Translit;
using Treynessen.Security;
using Treynessen.Localization;
using Treynessen.Database.Context;
using Treynessen.SettingsManagement;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped(provider => 
        new ConfigurationHandler("Settings/config.json", 
        services.BuildServiceProvider().GetRequiredService<IHostingEnvironment>()));

        services.AddDbContext<CMSDatabase>(options =>
        {
            ConfigurationHandler configHandler = services.BuildServiceProvider().GetRequiredService<ConfigurationHandler>();
            options.UseSqlServer(configHandler.Configuration["DBSettings:ConnectionString"]);
        });
        
        services.AddTransient(provider =>
        {
            ConfigurationHandler configHandler = services.BuildServiceProvider().GetRequiredService<ConfigurationHandler>();
            return new AccessLevelConfiguration(configHandler);
        });

        services.AddSingleton<EnRuTranslit>();

        services.AddTransient<ILoginFormLocalization>(provider => new RuLoginFormLocalization());
        services.AddTransient<IAdminPanelPageLocalization>(provider => new RuAdminPanelPageLocalization());
        services.AddTransient<IPagesLocalization>(provider => new RuPagesLocalization());
        services.AddTransient<IProductsLocalization>(provider => new RuProductsLocalization());
        services.AddTransient<ITemplatesLocalization>(provider => new RuTemplatesLocalization());
        services.AddTransient<IFileManagerLocalization>(provider => new RuFileManagerLocalization());

        services.AddMvc();
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
            // Страницы
            routeBuilder.MapRoute(
                name: "admin_panel",
                template: "~/admin",
                defaults: new { controller = "AdminPanel", action = "AdminPanel" }
            );
        });
    }
}