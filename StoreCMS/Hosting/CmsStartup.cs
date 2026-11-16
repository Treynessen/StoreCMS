using System;
using System.IO;
using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using Treynessen.Security.Installation.Automatic;
using Treynessen.Security.Installation.Bootstrap;
using Treynessen.SettingsManagement;

namespace Treynessen.Hosting;

public sealed class CmsStartupException : Exception
{
    public string Code { get; }
    internal CmsStartupException(string code, Exception inner = null) : base(code, inner) => Code = code;
}

public static class CmsStartup
{
    public static IHostBuilder CreateHostBuilder(string[] args) => Host.CreateDefaultBuilder(args)
        .ConfigureServices((context, services) =>
        {
            // [Комментарий ИИ-агента]: Imports готовы до настройки Razor и запуска установки, HTTP и фоновых служб.
            try { RazorViewImports.Initialize(context.HostingEnvironment.ContentRootPath); }
            catch (Exception error) when (error is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
            {
                throw new CmsStartupException("RazorViewImportsInitializationFailed/" + error.GetType().Name, error);
            }
            services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(context.HostingEnvironment.ContentRootPath,
                    "App_Data", "DataProtection")));
        })
        .ConfigureWebHostDefaults(web => web.UseWebRoot("Storage")
            .UseSetting(WebHostDefaults.ApplicationKey, typeof(Startup).Assembly.GetName().Name)
            .UseSetting(WebHostDefaults.PreventHostingStartupKey, "true")
            .UseStartup<Startup>());

    public static Task InitializeAsync(IServiceProvider services, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        using var scope = services.CreateScope();
        ConfigurationHandler configuration;
        try { configuration = scope.ServiceProvider.GetRequiredService<ConfigurationHandler>(); }
        catch (Exception) { throw new CmsStartupException("ConfigurationUnavailable"); }
        string connectionString = ValidateConfiguration(configuration);
        string language = configuration.GetConfigValue("LanguageSettings:CurrentLanguage");
        if (language != null)
        {
            try
            {
                var culture = new CultureInfo(language);
                CultureInfo.CurrentCulture = culture;
                CultureInfo.DefaultThreadCurrentCulture = culture;
            }
            catch (CultureNotFoundException) { }
        }
        return InitializeDatabaseAsync(connectionString, token);
    }

    private static async Task InitializeDatabaseAsync(string connectionString, CancellationToken token)
    {
        // [Комментарий ИИ-агента]: Согласованная автоматическая установка использует прежние receipt/marker и не восстанавливает завершённую установку.
        var result = await AutomaticPostgreSqlInstallation.RunAsync(connectionString,
            InitialInstallationPermission.AllowInitialInstallation, token,
            initialPassword: Environment.GetEnvironmentVariable("STORECMS_INITIAL_ADMIN_PASSWORD"));
        token.ThrowIfCancellationRequested();
        if (!result.CanStart) throw new CmsStartupException(result.Code + (result.DiagnosticCode == null ? "" : "/" + result.DiagnosticCode));
        Console.WriteLine("Состояние установки StoreCMS: " + result.Code);
    }

    public static string ValidateConfiguration(ConfigurationHandler configuration)
    {
        if (!string.Equals(configuration.DbConfiguration["Database"], "PostgreSQL", StringComparison.OrdinalIgnoreCase))
            throw new CmsStartupException("PostgreSqlProviderRequired");
        string value = configuration.DbConfiguration["ConnectionString"];
        if (string.IsNullOrWhiteSpace(value)) throw new CmsStartupException("ConnectionStringRequired");
        try
        {
            var settings = new NpgsqlConnectionStringBuilder(value);
            if (!IPAddress.TryParse(settings.Host, out _) || string.IsNullOrWhiteSpace(settings.Username)
                || !Regex.IsMatch(settings.Database ?? "", "^[a-zA-Z_][a-zA-Z0-9_]{0,62}$")
                || settings.Database is "postgres" or "template0" or "template1")
                throw new CmsStartupException("ExplicitTargetRequired");
        }
        catch (CmsStartupException) { throw; }
        catch (Exception) { throw new CmsStartupException("InvalidConnectionConfiguration"); }
        return value;
    }
}
