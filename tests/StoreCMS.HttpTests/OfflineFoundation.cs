using System.Net;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using StoreCMS.IntegrationTests;

namespace StoreCMS.HttpTests;

internal static class LiveOptIn
{
    internal static bool IsExplicit(string? enabled, string? stage) =>
        string.Equals(enabled, "1", StringComparison.Ordinal)
        && string.Equals(stage, "T-01.4.2", StringComparison.Ordinal);
}

internal sealed class OfflineLedger
{
    internal ResourceLedger State { get; }
    private bool stopped;

    internal OfflineLedger() => State = new ResourceLedger(Guid.NewGuid().ToString("N"), "Synthetic", "offline");

    private void Guard()
    {
        if (stopped) throw new InvalidOperationException("RunStopped");
    }

    internal void BeginCreate() { Guard(); State.BeginCreate(); }
    internal void AcknowledgeCreate() { Guard(); State.AcknowledgeCreate(); }
    internal void ObserveIdentity(uint oid) { Guard(); State.RecordOid(oid); }
    internal void ConfirmCreated() { Guard(); State.ConfirmCreated(); }
    internal void BeginDrop() { Guard(); State.BeginDrop(); }
    internal void AcknowledgeDrop() { Guard(); State.AcknowledgeDrop(); }

    internal void RecordSyntheticAbsentReadback(bool absent)
    {
        Guard();
        if (!absent) throw new InvalidOperationException("ObjectStillPresent");
        State.ConfirmAbsent();
    }

    internal void Stop()
    {
        stopped = true;
        State.Preserve();
    }
}

internal static class FutureHttpClient
{
    internal static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(20);

    internal static SocketsHttpHandler CreateHandler(X509Certificate2 authority) => new()
    {
        AllowAutoRedirect = false,
        UseProxy = false,
        ConnectTimeout = TimeSpan.FromSeconds(5),
        CookieContainer = new CookieContainer(),
        SslOptions = new SslClientAuthenticationOptions
        {
            CertificateChainPolicy = TestCertificates.Policy(authority)
        }
    };

    internal static HttpClient Create(X509Certificate2 authority) =>
        new(CreateHandler(authority)) { Timeout = RequestTimeout };
}

internal sealed class TestCertificates : IDisposable
{
    private readonly RSA authorityKey = RSA.Create(2048);
    private readonly RSA leafKey = RSA.Create(2048);
    internal X509Certificate2 Authority { get; }
    internal X509Certificate2 Leaf { get; }

    internal TestCertificates()
    {
        var now = DateTimeOffset.UtcNow;
        var ca = new CertificateRequest("CN=StoreCMS offline test CA", authorityKey, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        ca.CertificateExtensions.Add(new X509BasicConstraintsExtension(true, false, 0, true));
        ca.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.KeyCertSign, true));
        Authority = ca.CreateSelfSigned(now.AddMinutes(-5), now.AddHours(2));
        var request = new CertificateRequest("CN=localhost", leafKey, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        request.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, true));
        request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature, true));
        request.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(new OidCollection { new("1.3.6.1.5.5.7.3.1") }, true));
        var san = new SubjectAlternativeNameBuilder();
        san.AddDnsName("localhost");
        san.AddIpAddress(IPAddress.Loopback);
        request.CertificateExtensions.Add(san.Build());
        using var publicLeaf = request.Create(Authority, now.AddMinutes(-1), now.AddHours(1), RandomNumberGenerator.GetBytes(16));
        Leaf = publicLeaf.CopyWithPrivateKey(leafKey);
    }

    internal static X509ChainPolicy Policy(X509Certificate2 authority)
    {
        var policy = new X509ChainPolicy
        {
            TrustMode = X509ChainTrustMode.CustomRootTrust,
            RevocationMode = X509RevocationMode.NoCheck,
            DisableCertificateDownloads = true,
            VerificationFlags = X509VerificationFlags.NoFlag
        };
        policy.CustomTrustStore.Add(authority);
        policy.ApplicationPolicy.Add(new Oid("1.3.6.1.5.5.7.3.1"));
        return policy;
    }

    public void Dispose()
    {
        Leaf.Dispose();
        Authority.Dispose();
        leafKey.Dispose();
        authorityKey.Dispose();
    }
}