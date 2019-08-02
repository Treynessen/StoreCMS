using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Treynessen.Translit;
using Treynessen.Security;
using Treynessen.Extensions;
using Treynessen.Localization;
using Treynessen.Database.Context;
using Treynessen.RequestManagement;
using Treynessen.SettingsManagement;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped(provider =>
        {
            IHostingEnvironment env = services.BuildServiceProvider().GetRequiredService<IHostingEnvironment>();
            return new ConfigurationHandler(env.GetConfigsFolderFullPath() + "config.json");
        });

        services.AddDbContext<CMSDatabase>(options =>
        {
            ConfigurationHandler configHandler = services.BuildServiceProvider().GetRequiredService<ConfigurationHandler>();
            options.UseSqlServer(configHandler.DbConfiguration["ConnectionString"]);
        });

        services.AddTransient(provider =>
        {
            ConfigurationHandler configHandler = services.BuildServiceProvider().GetRequiredService<ConfigurationHandler>();
            return new AccessLevelConfiguration(configHandler.AccessConfiguration);
        });

        services.AddSingleton<EnRuTranslit>();

        services.AddTransient<ILoginFormLocalization>(provider => new RuLoginFormLocalization());
        services.AddTransient<IAdminPanelPageLocalization>(provider => new RuAdminPanelPageLocalization());
        services.AddTransient<IPagesLocalization>(provider => new RuPagesLocalization());
        services.AddTransient<ICategoriesAndProductsLocalization>(provider => new RuCategoriesAndProductsLocalization());
        services.AddTransient<IRedirectionsLocalization>(provider => new RuRedirectionsLocalization());
        services.AddTransient<ITemplatesLocalization>(provider => new RuTemplatesLocalization());
        services.AddTransient<IFileManagerLocalization>(provider => new RuFileManagerLocalization());
        services.AddTransient<IUserTypesLocalization>(provider => new RuUserTypesLocalization());
        services.AddTransient<ISynonymsForStringsLocalization>(provider => new RuSynonymsForStringsLocalization());
        services.AddTransient<IUserProfileLocalization>(provider => new RuUserProfileLocalization());
        services.AddTransient<ISettingsLocalization>(provider => new RuSettingsLocalization());

        services.AddTransient<IAdminPanelLogLocalization>(provider => new RuAdminPanelLogLocalization());

        services.AddMvc();
    }

    public void Configure(IApplicationBuilder app, IHostingEnvironment env)
    {
        //app.Use(async (context, next) =>
        //{
        //    context.Items["DateTime"] = System.DateTime.Now;
        //    await next.Invoke();
        //});
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        app.UseStaticFiles();
        app.UseMvc(routeBuilder =>
        {
            routeBuilder.MapRoute(
                name: "search_page",
                template: "~/search",
                defaults: new { controller = "Page", action = "SearchPage" }
            );
            routeBuilder.MapRoute(
                name: "some_page",
                template: "{*requestString}",
                defaults: new { controller = "Page", action = "RequestHandler" },
                constraints: new { requestString = new RequestConstraint() }
            );
            routeBuilder.MapRoute(
                name: "admin_panel",
                template: "~/admin",
                defaults: new { controller = "AdminPanel", action = "AdminPanel" }
            );
            routeBuilder.MapRoute(
                name: "page_not_found",
                template: "{*requestString}",
                defaults: new { controller = "Page", action = "PageNotFound" }
            );
        });
    }
}