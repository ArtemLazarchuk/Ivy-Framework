using System.Text;
using Isopoh.Cryptography.Argon2;
using Ivy.Tendril.Auth;
using Ivy.Tendril.Services;

namespace Ivy.Tendril.Test.Auth;

public class TendrilAuthProviderTests
{
    private static AuthConfig CreateAuthConfig(string password = "test-password", LoginRateLimitConfig? rateLimit = null)
    {
        var secret = GenerateSecret();
        var secretBytes = Convert.FromBase64String(secret);
        var salt = new byte[16];
        System.Security.Cryptography.RandomNumberGenerator.Fill(salt);
        var hash = Argon2.Hash(new Argon2Config
        {
            Type = Argon2Type.DataIndependentAddressing,
            Version = Argon2Version.Nineteen,
            TimeCost = 3,
            MemoryCost = 65536,
            Lanes = 1,
            Threads = 1,
            Password = Encoding.UTF8.GetBytes(password),
            Salt = salt,
            Secret = secretBytes,
            HashLength = 32
        });

        return new AuthConfig { Password = hash, HashSecret = secret, RateLimit = rateLimit };
    }

    private static string GenerateSecret()
    {
        var bytes = new byte[32];
        System.Security.Cryptography.RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes);
    }

    private static TendrilAuthProvider CreateProvider(AuthConfig auth)
    {
        var settings = new TendrilSettings { Auth = auth };
        var configService = new ConfigService(settings);
        return new TendrilAuthProvider(configService);
    }

    [Fact]
    public void PasswordMatches_CorrectPassword_ReturnsTrue()
    {
        var auth = CreateAuthConfig("my-secret-password");
        var provider = CreateProvider(auth);

        Assert.True(provider.PasswordMatches("my-secret-password"));
    }

    [Fact]
    public void PasswordMatches_IncorrectPassword_ReturnsFalse()
    {
        var auth = CreateAuthConfig("my-secret-password");
        var provider = CreateProvider(auth);

        Assert.False(provider.PasswordMatches("wrong-password"));
    }

    [Fact]
    public async Task LoginAsync_CorrectPassword_ReturnsToken()
    {
        var auth = CreateAuthConfig("login-test");
        var provider = CreateProvider(auth);

        var token = await provider.LoginAsync(null!, "anyone", "login-test", CancellationToken.None);

        Assert.NotNull(token);
        Assert.False(string.IsNullOrEmpty(token.AccessToken));
        Assert.False(string.IsNullOrEmpty(token.RefreshToken));
    }

    [Fact]
    public async Task LoginAsync_IncorrectPassword_ReturnsNull()
    {
        var auth = CreateAuthConfig("login-test");
        var provider = CreateProvider(auth);

        var token = await provider.LoginAsync(null!, "anyone", "bad-password", CancellationToken.None);

        Assert.Null(token);
    }

    [Fact]
    public void Constructor_MissingAuthConfig_Throws()
    {
        var settings = new TendrilSettings { Auth = null };
        var configService = new ConfigService(settings);

        Assert.Throws<InvalidOperationException>(() => new TendrilAuthProvider(configService));
    }

    [Fact]
    public void Constructor_InvalidHashSecret_Throws()
    {
        var auth = new AuthConfig { Password = "hash", HashSecret = "not-valid-base64!!!" };
        var settings = new TendrilSettings { Auth = auth };
        var configService = new ConfigService(settings);

        Assert.ThrowsAny<Exception>(() => new TendrilAuthProvider(configService));
    }

    [Fact]
    public void GetAuthOptions_ReturnsEmailPassword()
    {
        var auth = CreateAuthConfig();
        var provider = CreateProvider(auth);

        var options = provider.GetAuthOptions();

        Assert.Single(options);
        Assert.Equal(Ivy.AuthFlow.EmailPassword, options[0].Flow);
    }

    [Fact]
    public async Task LoginAsync_RateLimited_ReturnsNull()
    {
        var rateLimit = new LoginRateLimitConfig { Threshold = 1, BaseDelaySeconds = 100.0 };
        var auth = CreateAuthConfig("test-pass", rateLimit);
        var provider = CreateProvider(auth);

        // Two failed attempts to exceed threshold
        await provider.LoginAsync(null!, "anyone", "wrong", CancellationToken.None);
        await provider.LoginAsync(null!, "anyone", "wrong", CancellationToken.None);

        // Even correct password should be blocked
        var token = await provider.LoginAsync(null!, "anyone", "test-pass", CancellationToken.None);
        Assert.Null(token);
    }

    [Fact]
    public async Task LoginAsync_FailedAttempts_TriggersRateLimit()
    {
        var rateLimit = new LoginRateLimitConfig { Threshold = 2, BaseDelaySeconds = 100.0 };
        var auth = CreateAuthConfig("test-pass", rateLimit);
        var provider = CreateProvider(auth);

        // First 2 attempts are under threshold
        var t1 = await provider.LoginAsync(null!, "anyone", "wrong", CancellationToken.None);
        Assert.Null(t1);
        var t2 = await provider.LoginAsync(null!, "anyone", "wrong", CancellationToken.None);
        Assert.Null(t2);

        // 3rd attempt exceeds threshold — rate limited
        var t3 = await provider.LoginAsync(null!, "anyone", "wrong", CancellationToken.None);
        Assert.Null(t3);

        // Correct password is also blocked
        var t4 = await provider.LoginAsync(null!, "anyone", "test-pass", CancellationToken.None);
        Assert.Null(t4);
    }

    [Fact]
    public async Task LoginAsync_SuccessfulLogin_ClearsRateLimit()
    {
        var rateLimit = new LoginRateLimitConfig { Threshold = 2, BaseDelaySeconds = 0.0 };
        var auth = CreateAuthConfig("test-pass", rateLimit);
        var provider = CreateProvider(auth);

        // Fail a few times
        await provider.LoginAsync(null!, "anyone", "wrong", CancellationToken.None);
        await provider.LoginAsync(null!, "anyone", "wrong", CancellationToken.None);

        // Successful login resets counter
        var token = await provider.LoginAsync(null!, "anyone", "test-pass", CancellationToken.None);
        Assert.NotNull(token);

        // Subsequent login should work (counter was reset)
        var token2 = await provider.LoginAsync(null!, "anyone", "test-pass", CancellationToken.None);
        Assert.NotNull(token2);
    }

    [Fact]
    public async Task LoginAsync_CustomRateLimitConfig_Respected()
    {
        var rateLimit = new LoginRateLimitConfig { Threshold = 1, BaseDelaySeconds = 100.0, MaxDelaySeconds = 200.0 };
        var auth = CreateAuthConfig("test-pass", rateLimit);
        var provider = CreateProvider(auth);

        // 1 failure is under threshold
        await provider.LoginAsync(null!, "anyone", "wrong", CancellationToken.None);
        var t1 = await provider.LoginAsync(null!, "anyone", "test-pass", CancellationToken.None);
        Assert.NotNull(t1);

        // After success, counter is reset. Fail again to trigger rate limit.
        await provider.LoginAsync(null!, "anyone", "wrong", CancellationToken.None);
        await provider.LoginAsync(null!, "anyone", "wrong", CancellationToken.None);

        // Now blocked
        var t2 = await provider.LoginAsync(null!, "anyone", "test-pass", CancellationToken.None);
        Assert.Null(t2);
    }
}
