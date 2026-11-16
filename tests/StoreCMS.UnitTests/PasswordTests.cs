using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using StoreCMS.UnitTests.TestDoubles;
using Treynessen.Database.Entities;
using Treynessen.Security;
using Treynessen.Security.Installation;
using Xunit;

namespace StoreCMS.UnitTests;

[Trait("Category", "Unit")]
public sealed class PasswordTests : NetworkIsolatedTest
{
    [Theory]
    [InlineData("Correct-123!", "Wrong-123!")]
    [InlineData("Пароль-ёж🙂", "Пароль-еж🙂")]
    public void ProductionHashVerifiesOnlyCorrectPasswordAndIsIdentityV3Compatible(string password, string wrong)
    {
        string hash = SecurityFunctions.GetPasswordHash(password);
        var user = new User { Password = hash };

        Assert.True(SecurityFunctions.VerifyPassword(user, password));
        Assert.False(SecurityFunctions.VerifyPassword(user, wrong));
        Assert.True(InstallationEvaluator.IsSupportedPasswordHash(hash));
        Assert.Equal(1, Convert.FromBase64String(hash)[0]);
        var verifier = new PasswordHasher<User>(Options.Create(new PasswordHasherOptions
        {
            CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV3,
            IterationCount = 600000
        }));
        Assert.NotEqual(PasswordVerificationResult.Failed, verifier.VerifyHashedPassword(user, hash, password));
    }

    [Fact]
    public void SamePasswordProducesDifferentSaltedHashes()
    {
        string first = SecurityFunctions.GetPasswordHash("Synthetic-password");
        string second = SecurityFunctions.GetPasswordHash("Synthetic-password");
        Assert.NotEqual(first, second);
        Assert.True(SecurityFunctions.VerifyPassword(new User { Password = first }, "Synthetic-password"));
        Assert.True(SecurityFunctions.VerifyPassword(new User { Password = second }, "Synthetic-password"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not base64")]
    [InlineData("AAAA")]
    [InlineData("d41d8cd98f00b204e9800998ecf8427e")]
    public void MissingMalformedOrLegacyHashIsRejected(string? hash)
    {
        Assert.False(SecurityFunctions.VerifyPassword(new User { Password = hash! }, "Synthetic-password"));
        Assert.False(InstallationEvaluator.IsSupportedPasswordHash(hash!));
    }

    [Fact]
    public void TruncatedOrWrongVersionHashIsRejected()
    {
        byte[] payload = Convert.FromBase64String(SecurityFunctions.GetPasswordHash("Synthetic-password"));
        Assert.False(SecurityFunctions.VerifyPassword(new User { Password = Convert.ToBase64String(payload[..^1]) }, "Synthetic-password"));
        payload[0] = 0;
        Assert.False(SecurityFunctions.VerifyPassword(new User { Password = Convert.ToBase64String(payload) }, "Synthetic-password"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void NullOrEmptyCandidateCannotAuthenticate(string? candidate)
    {
        var user = new User { Password = SecurityFunctions.GetPasswordHash("Synthetic-password") };
        Assert.False(SecurityFunctions.VerifyPassword(user, candidate!));
    }

    [Fact]
    public void NullUserCannotAuthenticate()
    {
        Assert.False(SecurityFunctions.VerifyPassword(null!, "Synthetic-password"));
    }

    [Fact]
    public void HashingNullThrowsAndEmptyPasswordCannotAuthenticate()
    {
        Assert.Throws<ArgumentNullException>(() => SecurityFunctions.GetPasswordHash(null!));
        string hash = SecurityFunctions.GetPasswordHash("");
        Assert.True(InstallationEvaluator.IsSupportedPasswordHash(hash));
        Assert.False(SecurityFunctions.VerifyPassword(new User { Password = hash }, ""));
    }
}
