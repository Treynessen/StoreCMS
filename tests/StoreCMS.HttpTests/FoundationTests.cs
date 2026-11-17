using System.Security.Cryptography.X509Certificates;
using StoreCMS.IntegrationTests;
using StoreCMS.UnitTests.TestDoubles;
using Xunit;

namespace StoreCMS.HttpTests;

[Trait("Category", "OfflineHttp")]
public sealed class OptInTests : NetworkIsolatedTest
{
    [Theory]
    [InlineData(null, null)]
    [InlineData("", "T-01.4.2")]
    [InlineData("0", "T-01.4.2")]
    [InlineData("true", "T-01.4.2")]
    [InlineData(" 1", "T-01.4.2")]
    [InlineData("1", null)]
    [InlineData("1", "")]
    [InlineData("1", "T-01.3")]
    [InlineData("1", "T-01.4.1")]
    [InlineData("1", "T-01.4.2 ")]
    public void NonExplicitPermissionIsRejected(string? enabled, string? stage) =>
        Assert.False(LiveOptIn.IsExplicit(enabled, stage));

    [Fact]
    public void ExactFutureStagePermissionIsRecognizedWithoutRunningAnything() =>
        Assert.True(LiveOptIn.IsExplicit("1", "T-01.4.2"));
}

[Trait("Category", "OfflineHttp")]
public sealed class LedgerTests : NetworkIsolatedTest
{
    private static OfflineLedger Created()
    {
        var ledger = new OfflineLedger();
        ledger.BeginCreate();
        ledger.AcknowledgeCreate();
        ledger.ObserveIdentity(42);
        ledger.ConfirmCreated();
        return ledger;
    }

    [Fact]
    public void UnknownCreatePreservesAndBlocksEveryMutation()
    {
        var ledger = new OfflineLedger();
        ledger.BeginCreate();
        ledger.Stop();
        Assert.Equal(CreationState.UnknownCreateOutcome, ledger.State.Creation);
        Assert.True(ledger.State.Preserved);
        Assert.Throws<InvalidOperationException>(ledger.BeginCreate);
        Assert.Throws<InvalidOperationException>(ledger.AcknowledgeCreate);
        Assert.Throws<InvalidOperationException>(() => ledger.ObserveIdentity(42));
        Assert.Throws<InvalidOperationException>(ledger.ConfirmCreated);
        Assert.Throws<InvalidOperationException>(ledger.BeginDrop);
    }

    [Fact]
    public void UnknownDropPreservesAndCannotBeRetriedOrConfirmedAfterStop()
    {
        var ledger = Created();
        ledger.BeginDrop();
        ledger.Stop();
        Assert.Equal(DeletionState.UnknownDropOutcome, ledger.State.Deletion);
        Assert.True(ledger.State.Preserved);
        Assert.Throws<InvalidOperationException>(ledger.BeginDrop);
        Assert.Throws<InvalidOperationException>(ledger.AcknowledgeDrop);
        Assert.Throws<InvalidOperationException>(() => ledger.RecordSyntheticAbsentReadback(true));
        Assert.Equal(DeletionState.UnknownDropOutcome, ledger.State.Deletion);
    }

    [Fact]
    public void AcknowledgmentRequiresSeparateSyntheticIdentityAndAbsentReadback()
    {
        var ledger = new OfflineLedger();
        ledger.BeginCreate();
        ledger.AcknowledgeCreate();
        Assert.Equal(CreationState.CreateAcknowledged, ledger.State.Creation);
        Assert.Null(ledger.State.Oid);
        Assert.Throws<InvalidOperationException>(ledger.ConfirmCreated);
        ledger.ObserveIdentity(42);
        ledger.ConfirmCreated();
        ledger.BeginDrop();
        Assert.Throws<InvalidOperationException>(() => ledger.RecordSyntheticAbsentReadback(true));
        ledger.AcknowledgeDrop();
        Assert.Equal(DeletionState.DropAcknowledged, ledger.State.Deletion);
        Assert.Throws<InvalidOperationException>(() => ledger.RecordSyntheticAbsentReadback(false));
        Assert.Equal(DeletionState.DropAcknowledged, ledger.State.Deletion);
        ledger.RecordSyntheticAbsentReadback(true);
        Assert.Equal(DeletionState.AbsentReadbackConfirmed, ledger.State.Deletion);
    }

    [Fact]
    public void RepeatedCreateAndDropAreRejected()
    {
        var ledger = Created();
        Assert.Throws<InvalidOperationException>(ledger.BeginCreate);
        Assert.Throws<InvalidOperationException>(ledger.AcknowledgeCreate);
        ledger.BeginDrop();
        Assert.Throws<InvalidOperationException>(ledger.BeginDrop);
        ledger.AcknowledgeDrop();
        Assert.Throws<InvalidOperationException>(ledger.AcknowledgeDrop);
    }

    [Fact]
    public void StopBeforeCreateAndAfterAcknowledgedDropDoesNotGrantCleanup()
    {
        var before = new OfflineLedger();
        before.Stop();
        Assert.Throws<InvalidOperationException>(before.BeginCreate);
        var after = Created();
        after.BeginDrop();
        after.AcknowledgeDrop();
        after.Stop();
        Assert.Equal(DeletionState.DropAcknowledged, after.State.Deletion);
        Assert.True(after.State.Preserved);
        Assert.Throws<InvalidOperationException>(() => after.RecordSyntheticAbsentReadback(true));
    }
}

[Trait("Category", "OfflineHttp")]
public sealed class TokenTests : NetworkIsolatedTest
{
    private static string Form(string inputs) =>
        "<form id=\"login-form\" method=\"post\" data-admin-ajax>"
        + "<input type=\"hidden\" name=\"pageID\" value=\""
        + (int)Treynessen.AdminPanelTypes.AdminPanelPages.LoginForm + "\">"
        + inputs + "</form>";

    private const string Input = "<input type=\"hidden\" name=\"__RequestVerificationToken\" value=\"synthetic&amp;&#43;&#x2f;\">";

    [Fact]
    public void KnownLoginFormReturnsOneHtmlDecodedTokenWithoutDiagnosticValues()
    {
        bool matches = string.Equals("synthetic&+/", CsrfTokenExtractor.ExtractLoginToken(Form(Input)), StringComparison.Ordinal);
        Assert.True(matches, "DecodedTokenMismatch");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    [InlineData(8)]
    [InlineData(9)]
    [InlineData(10)]
    [InlineData(11)]
    public void InvalidFormsFailWithOnlySafeCode(int scenario)
    {
        string html = scenario switch
        {
            0 => Form(""),
            1 => Form(Input + Input),
            2 => Form(Input).Replace("</form>", ""),
            3 => Form(Input).Replace("login-form", "other-form"),
            4 => Form(Input).Replace("method=\"post\"", "method=\"get\""),
            5 => Form(Input).Replace("data-admin-ajax", "action=\"https://example.invalid/\""),
            6 => Form(Input).Replace("value=\"synthetic", "value=synthetic"),
            7 => Form(Input).Replace("name=\"__RequestVerificationToken\"", "name=\"__RequestVerificationToken\" name=\"other\""),
            8 => Form(Input).Replace("synthetic&amp;&#43;&#x2f;", ""),
            9 => "<!--" + Form(Input) + "-->",
            10 => "<script>" + Form(Input) + "</script>",
            11 => Form(Input) + Form(Input),
            _ => throw new InvalidOperationException("UnknownSyntheticCase")
        };
        var error = Assert.Throws<InvalidOperationException>(() => CsrfTokenExtractor.ExtractLoginToken(html));
        Assert.True(error.Message == "CsrfFormRejected");
    }

    [Fact]
    public void TokenInAnotherFormDoesNotSatisfyLoginForm() =>
        Assert.Throws<InvalidOperationException>(() => CsrfTokenExtractor.ExtractLoginToken(
            "<form id=\"other\" method=\"post\">" + Input + "</form>" + Form("")));

    [Fact]
    public void MetaTokenDoesNotCountAsSecondHiddenFormToken() =>
        Assert.True(CsrfTokenExtractor.ExtractLoginToken(
            "<meta name=\"csrf-token\" content=\"other-synthetic\">" + Form(Input)).Length > 0);
}

[Trait("Category", "OfflineHttp")]
public sealed class TlsClientTests : NetworkIsolatedTest
{
    [Fact]
    public void OwnAuthorityBuildsServerAuthChainOnlyInMemory()
    {
        using var certificates = new TestCertificates();
        using var chain = new X509Chain { ChainPolicy = TestCertificates.Policy(certificates.Authority) };
        Assert.True(certificates.Leaf.HasPrivateKey);
        Assert.True(chain.Build(certificates.Leaf), "SyntheticChainRejected");
        Assert.True(chain.ChainElements[^1].Certificate.RawData.AsSpan().SequenceEqual(certificates.Authority.RawData));
    }

    [Fact]
    public void ForeignAuthorityIsRejectedWithoutDownloads()
    {
        using var certificates = new TestCertificates();
        using var foreign = new TestCertificates();
        using var chain = new X509Chain { ChainPolicy = TestCertificates.Policy(foreign.Authority) };
        Assert.False(chain.Build(certificates.Leaf));
        Assert.NotEmpty(chain.ChainStatus);
    }

    [Fact]
    public void ClientHasBoundedTimeoutsNoRedirectProxyOrCertificateBypass()
    {
        using var certificates = new TestCertificates();
        using var handler = FutureHttpClient.CreateHandler(certificates.Authority);
        using var client = FutureHttpClient.Create(certificates.Authority);
        Assert.False(handler.AllowAutoRedirect);
        Assert.False(handler.UseProxy);
        Assert.Null(handler.Proxy);
        Assert.InRange(handler.ConnectTimeout.TotalSeconds, 1, 10);
        Assert.InRange(client.Timeout.TotalSeconds, 1, 30);
        Assert.Null(handler.SslOptions.RemoteCertificateValidationCallback);
        Assert.Null(handler.SslOptions.LocalCertificateSelectionCallback);
        var policy = handler.SslOptions.CertificateChainPolicy!;
        Assert.Equal(X509VerificationFlags.NoFlag, policy.VerificationFlags);
        Assert.Equal(X509ChainTrustMode.CustomRootTrust, policy.TrustMode);
        Assert.True(policy.DisableCertificateDownloads);
        Assert.Single(policy.CustomTrustStore);
        Assert.Null(client.BaseAddress);
    }
}