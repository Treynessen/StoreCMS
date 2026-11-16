using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
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
            IWebHostEnvironment env = provider.GetRequiredService<IWebHostEnvironment>();
            return new ConfigurationHandler(env.GetConfigsFolderFullPath() + "config.json");
        });

        services.AddDbContext<CMSDatabase>((provider, options) =>
        {
            ConfigurationHandler configHandler = provider.GetRequiredService<ConfigurationHandler>();
            options.UseNpgsql(configHandler.DbConfiguration["ConnectionString"]);
        });

        services.AddTransient(provider =>
        {
            ConfigurationHandler configHandler = provider.GetRequiredService<ConfigurationHandler>();
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

        services.AddSingleton<TimedHostedService>();
        services.AddHostedService(provider => provider.GetRequiredService<TimedHostedService>());

        AdminAntiforgery.Configure(services);
        services.AddMvc(options => options.EnableEndpointRouting = false).AddRazorRuntimeCompilation();
        services.PostConfigure<Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation.MvcRazorRuntimeCompilationOptions>(options =>
            Treynessen.TemplatesManagement.GeneratedRazorFileProvider.WrapPhysicalProviders(options.FileProviders));

        // [Комментарий ИИ-агента]: При прямом доступе схема и адрес клиента определяются транспортом, а не forwarded-заголовками клиента.
        services.Configure<ForwardedHeadersOptions>(options => options.ForwardedHeaders = ForwardedHeaders.None);
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory, IHttpContextAccessor httpContextAccessor)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        loggerFactory.AddFile(env.GetLogsFolderFullPath(), "errors.log", httpContextAccessor);
        app.UseForwardedHeaders();
        app.UseSitemap();
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
