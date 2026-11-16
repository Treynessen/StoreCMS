using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StoreCMS.UnitTests.TestDoubles;
using Treynessen.AdminPanelTypes;
using Treynessen.Database.Entities;
using Treynessen.Security;
using Xunit;

namespace StoreCMS.UnitTests;

[Trait("Category", "Unit")]
public sealed class AdminGuardTests : NetworkIsolatedTest
{
    public static IEnumerable<object?[]> ForcedChangeMatrix()
    {
        var selectors = Enum.GetValues<AdminPanelPages>().Cast<AdminPanelPages?>().Concat([null, (AdminPanelPages)int.MaxValue]);
        foreach (var page in selectors)
            foreach (string method in new[] { "GET", "POST", "PUT", "DELETE", "HEAD", "OPTIONS", "PATCH" })
                yield return [method, page];
    }

    [Theory]
    [MemberData(nameof(ForcedChangeMatrix))]
    public async Task ForcedChangeAllowsOnlyChangeOrExitAndNeverOrdinaryAction(string method, AdminPanelPages? page)
    {
        using var fixture = new AdminGuardFixture(method, page);
        var result = await fixture.Invoke();
        bool allowed = (method == "POST" && page == AdminPanelPages.ChangeInitialPassword)
            || (method == "DELETE" && page == AdminPanelPages.Exit);

        Assert.Equal("no-store", fixture.Controller.Response.Headers.CacheControl.ToString());
        if (allowed)
        {
            Assert.True(fixture.ActionRan);
            Assert.Null(result);
            Assert.Equal(1, fixture.Antiforgery.Calls);
        }
        else
        {
            Assert.False(fixture.ActionRan);
            Assert.Equal(0, fixture.Antiforgery.Calls);
            if (method == "GET")
                Assert.Equal("ChangeInitialPassword", Assert.IsType<ViewResult>(result).ViewName);
            else if (result is StatusCodeResult status)
                Assert.Equal(400, status.StatusCode);
            else
                Assert.Equal(403, Assert.IsType<ObjectResult>(result).StatusCode);
        }
        var user = new User { RequirePasswordChange = true, UserType = new UserType { AccessLevel = AccessLevel.VeryHigh } };
        Assert.False(SecurityFunctions.HasAccessTo(page, user, new DefaultHttpContext()));
    }

    [Theory]
    [InlineData("POST", AdminPanelPages.ChangeInitialPassword)]
    [InlineData("DELETE", AdminPanelPages.Exit)]
    [InlineData("POST", AdminPanelPages.EditSettings)]
    public async Task HttpIsRejectedBeforeCsrfOrAction(string method, AdminPanelPages page)
    {
        using var fixture = new AdminGuardFixture(method, page, https: false);
        fixture.Antiforgery.Failure = new IOException("must-not-be-called");

        Assert.Equal(403, Assert.IsType<ObjectResult>(await fixture.Invoke()).StatusCode);
        Assert.Equal(0, fixture.Antiforgery.Calls);
        Assert.False(fixture.ActionRan);
    }

    [Theory]
    [InlineData("POST", AdminPanelPages.ChangeInitialPassword, true)]
    [InlineData("DELETE", AdminPanelPages.Exit, true)]
    [InlineData("POST", AdminPanelPages.EditSettings, false)]
    [InlineData("PUT", AdminPanelPages.UploadFile, false)]
    public async Task InvalidCsrfStopsPermittedMutation(string method, AdminPanelPages page, bool required)
    {
        using var fixture = new AdminGuardFixture(method, page, required);
        fixture.Antiforgery.Valid = false;

        Assert.Equal(400, Assert.IsType<StatusCodeResult>(await fixture.Invoke()).StatusCode);
        Assert.Equal(1, fixture.Antiforgery.Calls);
        Assert.False(fixture.ActionRan);
    }

    [Theory]
    [InlineData("PageId")]
    [InlineData("model.PageId")]
    public async Task InvalidSelectorModelStopsBeforeCsrf(string key)
    {
        using var fixture = new AdminGuardFixture("POST", AdminPanelPages.ChangeInitialPassword);
        fixture.Controller.ModelState.AddModelError(key, "synthetic invalid selector");

        Assert.Equal(400, Assert.IsType<StatusCodeResult>(await fixture.Invoke()).StatusCode);
        Assert.Equal(0, fixture.Antiforgery.Calls);
        Assert.False(fixture.ActionRan);
    }

    [Theory]
    [InlineData("POST", AdminPanelPages.EditSettings)]
    [InlineData("PUT", AdminPanelPages.UploadFile)]
    [InlineData("DELETE", AdminPanelPages.DeletePage)]
    public async Task ReadyAdministratorWithValidCsrfReachesDelegate(string method, AdminPanelPages page)
    {
        using var fixture = new AdminGuardFixture(method, page, required: false);
        Assert.Null(await fixture.Invoke());
        Assert.True(fixture.ActionRan);
        Assert.Equal(1, fixture.Antiforgery.Calls);
    }

    [Fact]
    public async Task UnauthenticatedMutationStopsBeforeCsrf()
    {
        using var fixture = new AdminGuardFixture("POST", AdminPanelPages.EditSettings, authenticated: false);
        Assert.Equal(401, Assert.IsType<StatusCodeResult>(await fixture.Invoke()).StatusCode);
        Assert.Equal(0, fixture.Antiforgery.Calls);
        Assert.False(fixture.ActionRan);
    }

    [Fact]
    public async Task InsufficientAccessStopsBeforeCsrf()
    {
        using var fixture = new AdminGuardFixture("POST", AdminPanelPages.EditSettings, required: false, level: AccessLevel.Low);
        Assert.Equal(403, Assert.IsType<StatusCodeResult>(await fixture.Invoke()).StatusCode);
        Assert.Equal(0, fixture.Antiforgery.Calls);
        Assert.False(fixture.ActionRan);
    }

    [Theory]
    [InlineData("POST", AdminPanelPages.LoginForm)]
    [InlineData("DELETE", AdminPanelPages.Exit)]
    public async Task AnonymousLoginAndLogoutStillRequireCsrf(string method, AdminPanelPages page)
    {
        using var fixture = new AdminGuardFixture(method, page, authenticated: false);
        fixture.Antiforgery.Valid = false;
        Assert.Equal(400, Assert.IsType<StatusCodeResult>(await fixture.Invoke()).StatusCode);
        Assert.Equal(1, fixture.Antiforgery.Calls);
        Assert.False(fixture.ActionRan);
    }

    [Fact]
    public async Task DependencyFailureDoesNotExecuteMutationOrBecomeSuccess()
    {
        using var fixture = new AdminGuardFixture("POST", AdminPanelPages.EditSettings, required: false);
        fixture.Antiforgery.Failure = new IOException("synthetic dependency failure");
        await Assert.ThrowsAsync<IOException>(() => fixture.Invoke());
        Assert.False(fixture.ActionRan);
        Assert.Equal(1, fixture.Antiforgery.Calls);
    }
}
