using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Treynessen.Translit;
using Treynessen.Security;
using Treynessen.Extensions;
using Treynessen.Localization;
using Treynessen.Database.Context;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddDbContext<CMSDatabase>(options =>
        {
            var builder = new ConfigurationBuilder()
            .SetBasePath(services.BuildServiceProvider().GetRequiredService<IHostingEnvironment>().ContentRootPath)
            .AddJsonFile("Configs/core_configuration.json");
            options.UseSqlServer(builder.Build().GetConnectionString("DefaultConnection"));
        });
        
        services.AddTransient(provider =>
        {
            string pathToAccessConfig = $"{provider.GetRequiredService<IHostingEnvironment>().GetConfigsFolderFullPath()}accessLevel_configuration.json";
            return new AccessLevelConfiguration(pathToAccessConfig);
        });

        services.AddSingleton<EnRuTranslit>();

        services.AddTransient<ILoginFormLocalization>(provider => new RuLoginFormLocalization());
        services.AddTransient<IAdminPanelPageLocalization>(provider => new RuAdminPanelPageLocalization());
        services.AddTransient<IPagesLocalization>(provider => new RuPagesLocalization());
        services.AddTransient<IProductsLocalization>(provider => new RuProductsLocalization());
        services.AddTransient<ITemplatesLocalization>(provider => new RuTemplatesLocalization());

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