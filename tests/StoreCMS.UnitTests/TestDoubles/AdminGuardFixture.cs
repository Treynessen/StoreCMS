using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Treynessen.AdminPanelTypes;
using Treynessen.Controllers;
using Treynessen.Database.Entities;
using Treynessen.Security;

namespace StoreCMS.UnitTests.TestDoubles;

internal sealed class AdminGuardFixture : IDisposable
{
    private readonly ServiceProvider services;
    internal AntiforgeryDouble Antiforgery { get; } = new();
    internal AdminPanelController Controller { get; }
    internal ActionExecutingContext Context { get; }
    internal bool ActionRan { get; private set; }

    internal AdminGuardFixture(string method, AdminPanelPages? page, bool required = true,
        bool https = true, bool authenticated = true, AccessLevel level = AccessLevel.VeryHigh)
    {
        var registrations = new ServiceCollection();
        registrations.AddLogging();
        registrations.AddControllersWithViews();
        registrations.AddSingleton(new AccessLevelConfiguration(new ConfigurationBuilder().Build().GetSection("Access")));
        registrations.AddSingleton<IAntiforgery>(Antiforgery);
        services = registrations.BuildServiceProvider();

        var http = new DefaultHttpContext { RequestServices = services };
        http.Request.Scheme = https ? "https" : "http";
        http.Request.Method = method;
        http.Items["StoreCMS.AuthenticatedUserChecked"] = true;
        http.Items["User"] = authenticated
            ? new User { ID = 42, RequirePasswordChange = required, UserType = new UserType { AccessLevel = level } }
            : null;
        Controller = new AdminPanelController(null!, null!);
        var action = new ActionContext(http, new RouteData(), new ControllerActionDescriptor(), new ModelStateDictionary());
        Controller.ControllerContext = new ControllerContext(action);
        Context = new ActionExecutingContext(action, [], new Dictionary<string, object?>
        {
            ["model"] = new Model { PageId = page },
            ["pageID"] = page
        }, Controller);
    }

    internal async Task<IActionResult?> Invoke()
    {
        await Controller.OnActionExecutionAsync(Context, () =>
        {
            ActionRan = true;
            return Task.FromResult(new ActionExecutedContext(Context, [], Controller) { Result = new StatusCodeResult(204) });
        });
        return Context.Result;
    }

    public void Dispose() => services.Dispose();
}

internal sealed class AntiforgeryDouble : IAntiforgery
{
    internal bool Valid { get; set; } = true;
    internal Exception? Failure { get; set; }
    internal int Calls { get; private set; }

    public Task ValidateRequestAsync(HttpContext context)
    {
        Calls++;
        if (Failure != null) return Task.FromException(Failure);
        return Valid ? Task.CompletedTask : Task.FromException(new AntiforgeryValidationException("Synthetic invalid token"));
    }

    public Task<bool> IsRequestValidAsync(HttpContext context) => throw new NotSupportedException();
    public AntiforgeryTokenSet GetAndStoreTokens(HttpContext context) => throw new NotSupportedException();
    public AntiforgeryTokenSet GetTokens(HttpContext context) => throw new NotSupportedException();
    public void SetCookieTokenAndHeader(HttpContext context) => throw new NotSupportedException();
}
