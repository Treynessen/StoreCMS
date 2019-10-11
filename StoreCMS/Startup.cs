using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;
using Treynessen;
using Treynessen.Translit;
using Treynessen.Security;
using Treynessen.Extensions;
using Treynessen.Middlewares;
using Treynessen.Localization;
using Treynessen.LogManagement;
using Treynessen.Database.Context;
using Treynessen.RequestManagement;
using Treynessen.SettingsManagement;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

        services.AddScoped(provider =>
        {
            IHostingEnvironment env = services.BuildServiceProvider().GetRequiredService<IHostingEnvironment>();
            return new ConfigurationHandler(env.GetConfigsFolderFullPath() + "config.json");
        });

        services.AddDbContext<CMSDatabase>(options =>
        {
            ConfigurationHandler configHandler = services.BuildServiceProvider().GetRequiredService<ConfigurationHandler>();
            switch (configHandler.DbConfiguration["Database"].ToLower())
            {
                case "mssql":
                    options.UseSqlServer(configHandler.DbConfiguration["ConnectionString"]);
                    break;
                case "mysql":
                    options.UseMySql(configHandler.DbConfiguration["ConnectionString"]);
                    break;
            }
        });

        services.AddTransient(provider =>
        {
            ConfigurationHandler configHandler = services.BuildServiceProvider().GetRequiredService<ConfigurationHandler>();
            return new AccessLevelConfiguration(configHandler.AccessConfiguration);
        });

        services.AddSingleton<EnRuTranslit>();

        services.AddTransient<ILoginFormLocalization>(provider => new RuLoginFormLocalization());
        services.AddTransient<IAdminPanelPageLocalization>(provider => new RuAdminPanelPageLocalization());
        services.AddTransient<IMainPageLocalization>(provider => new RuMainPageLocalization());
        services.AddTransient<IVisitorsLocalization>(provider => new RuVisitorsLocalization());
        services.AddTransient<IPagesLocalization>(provider => new RuPagesLocalization());
        services.AddTransient<ICategoriesAndProductsLocalization>(provider => new RuCategoriesAndProductsLocalization());
        services.AddTransient<IRedirectionsLocalization>(provider => new RuRedirectionsLocalization());
        services.AddTransient<ITemplatesLocalization>(provider => new RuTemplatesLocalization());
        services.AddTransient<IFileManagerLocalization>(provider => new RuFileManagerLocalization());
        services.AddTransient<IUsersLocalization>(provider => new RuUsersLocalization());
        services.AddTransient<IUserActionsLocalization>(provider => new RuUserActionsLocalization());
        services.AddTransient<IUserTypesLocalization>(provider => new RuUserTypesLocalization());
        services.AddTransient<ISynonymsForStringsLocalization>(provider => new RuSynonymsForStringsLocalization());
        services.AddTransient<IUserProfileLocalization>(provider => new RuUserProfileLocalization());
        services.AddTransient<ISettingsLocalization>(provider => new RuSettingsLocalization());

        services.AddTransient<IAdminPanelLogLocalization>(provider => new RuAdminPanelLogLocalization());

        services.AddHostedService<TimedHostedService>();

        services.AddMvc();

        services.Configure<ForwardedHeadersOptions>(options => options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto);
    }

    public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IHttpContextAccessor httpContextAccessor)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        loggerFactory.AddFile(env.GetLogsFolderFullPath(), "errors.log", httpContextAccessor);
        app.UseSitemap();
        app.UseForwardedHeaders();
        app.UseForcedGarbageCollection();
        app.UseStaticFiles(new StaticFileOptions() { OnPrepareResponse = sfr => sfr.Context.Response.Headers.Add("Cache-Control", "private, max-age=604800") });
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