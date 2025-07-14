using Mashkoor.Core.Data;
using Mashkoor.Core.Security.Totp;
using Mashkoor.Core.Security.Totp.Configuration;
using Mashkoor.Core.Test.Common;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Time.Testing;

namespace Mashkoor.Core.Test.Security.Totp;

public class TotpTokenProviderTests
{
    private static readonly TotpConfig _config = new()
    {
        UseDefaultOtp = false,
        Duration = TimeSpan.FromMinutes(3),
    };
    private static readonly TotpConfig _defaultOtpConfig = new()
    {
        UseDefaultOtp = true,
        DefaultOtp = "1234",
        Duration = TimeSpan.FromMinutes(3),
    };

    [Fact]
    public void Generate_generates_code_of_size_4()
    {
        // Arrange
        var provider = new TotpTokenProvider(_config, TimeProvider.System, new MemoryCache(new MemoryCacheOptions()));
        var user = new User { Id = 1, SecurityStamp = Guid.NewGuid().ToString() };

        // Act
        var code = provider.Generate(user, TotpPurpose.SignInPurpose);

        // Assert
        Assert.Equal(4, code.Length);
    }

    [Fact]
    public void Generate_generates_different_code_when_different_user()
    {
        // Arrange
        var time = new DateTime(2022, 3, 28, 0, 0, 0, DateTimeKind.Utc);
        var timeProviderMoq = new FakeTimeProvider();
        timeProviderMoq.SetUtcNow(time);

        var provider = new TotpTokenProvider(_config, timeProviderMoq, new MemoryCache(new MemoryCacheOptions()));

        var sts = Guid.NewGuid().ToString();
        var user1 = new User { Id = 1, SecurityStamp = sts };
        var user2 = new User { Id = 2, SecurityStamp = sts };

        // Act
        var code1 = provider.Generate(user1, TotpPurpose.SignInPurpose);
        var code2 = provider.Generate(user2, TotpPurpose.SignInPurpose);

        // Assert
        Assert.NotEqual(code1, code2);
    }

    [Fact]
    public void Generate_generates_different_code_when_same_user_but_different_purpose()
    {
        // Arrange
        var time = new DateTime(2022, 3, 28, 0, 0, 0, DateTimeKind.Utc);
        var timeProviderMoq = new FakeTimeProvider();
        timeProviderMoq.SetUtcNow(time);

        var provider = new TotpTokenProvider(_config, timeProviderMoq, new MemoryCache(new MemoryCacheOptions()));

        var sts = Guid.NewGuid().ToString();
        var user = new User { Id = 1, SecurityStamp = sts };
        var purpose1 = "signin";
        var purpose2 = "enroll";

        // Act
        var code1 = provider.Generate(user, purpose1);
        var code2 = provider.Generate(user, purpose2);

        // Assert
        Assert.NotEqual(code1, code2);
    }

    [Fact]
    public void Generate_generates_different_code_when_securityTimestamp_changes_for_same_user()
    {
        // Arrange
        var time = new DateTime(2022, 3, 28, 0, 0, 0, DateTimeKind.Utc);
        var timeProviderMoq = new FakeTimeProvider();
        timeProviderMoq.SetUtcNow(time);

        var provider = new TotpTokenProvider(_config, timeProviderMoq, new MemoryCache(new MemoryCacheOptions()));

        var user = new User { Id = 1 };

        // Act
        user.SecurityStamp = Guid.NewGuid().ToString();
        var code1 = provider.Generate(user, TotpPurpose.SignInPurpose);

        user.SecurityStamp = Guid.NewGuid().ToString();
        var code2 = provider.Generate(user, TotpPurpose.SignInPurpose);

        // Assert
        Assert.NotEqual(code1, code2);
    }

    [Fact]
    public void Generates_same_code_within_configured_timespan()
    {
        var time = new DateTime(2022, 3, 28, 0, 0, 0, DateTimeKind.Utc);
        var timeProviderMoq = new FakeTimeProvider();
        timeProviderMoq.SetUtcNow(time);

        var provider = new TotpTokenProvider(_config, timeProviderMoq, new MemoryCache(new MemoryCacheOptions()));
        var user = new User { Id = 1, SecurityStamp = Guid.NewGuid().ToString() };
        string prevOtp = null;

        for (var i = 0; i < _config.Duration.TotalSeconds; i++)
        {
            var otp = provider.Generate(user, TotpPurpose.SignInPurpose);
            Assert.True(prevOtp is null || prevOtp == otp);
            prevOtp = otp;
            timeProviderMoq.Advance(TimeSpan.FromSeconds(1));
        }
    }

    [Fact]
    public void Generated_code_is_valid_within_configured_timespan()
    {
        var time = new DateTime(2022, 3, 28, 0, 0, 0, DateTimeKind.Utc);
        var timeProviderMoq = new MutableTimeProvider();

        var provider = new TotpTokenProvider(_config, timeProviderMoq, new MemoryCache(new MemoryCacheOptions()));
        var user = new User { Id = 1, SecurityStamp = Guid.NewGuid().ToString() };

        for (var i = 0; i < _config.Duration.TotalSeconds; i++)
        {
            var otpTime = time.AddSeconds(i);
            timeProviderMoq.SetUtcNow(otpTime);
            var otp = provider.Generate(user, TotpPurpose.SignInPurpose);

            for (var j = -_config.Duration.TotalSeconds; j <= _config.Duration.TotalSeconds; j++)
            {
                var checkTime = otpTime.AddSeconds(j);
                timeProviderMoq.SetUtcNow(checkTime);
                var isValid = provider.Validate(otp, user, TotpPurpose.SignInPurpose);

                if (j >= 0)
                {
                    Assert.True(isValid);
                }
                else
                {
                    if (Math.Abs(j) <= i)
                    {
                        Assert.True(isValid);
                    }
                    else
                    {
                        Assert.False(isValid);
                    }
                }
            }

            timeProviderMoq.SetUtcNow(otpTime.AddSeconds(_config.Duration.TotalSeconds * 2));
            Assert.False(provider.Validate(otp, user, TotpPurpose.SignInPurpose));
        }
    }

    [Fact]
    public void TryGenerate_returns_true_when_no_valid_prev_generated_code_exist()
    {
        // Arrange
        var provider = new TotpTokenProvider(_config, TimeProvider.System, new MemoryCache(new MemoryCacheOptions()));
        var user = new User { Id = 1, SecurityStamp = Guid.NewGuid().ToString() };

        // Act
        var result = provider.TryGenerate(user, TotpPurpose.SignInPurpose, out var code);

        // Assert
        Assert.True(result);
        Assert.NotNull(code);
    }

    [Fact]
    public void TryGenerate_returns_false_when_a_valid_prev_generated_code_exist()
    {
        // Arrange
        var provider = new TotpTokenProvider(_config, TimeProvider.System, new MemoryCache(new MemoryCacheOptions()));
        var user = new User { Id = 1, SecurityStamp = Guid.NewGuid().ToString() };

        // Act
        provider.TryGenerate(user, TotpPurpose.SignInPurpose, out _);
        var result = provider.TryGenerate(user, TotpPurpose.SignInPurpose, out var code);

        // Assert
        Assert.False(result);
        Assert.Null(code);
    }

    [Fact]
    public void Generate_adds_cache_entry_that_expires_after_configured_timespan()
    {
        // Arrange
        var purpose = TotpPurpose.SignInPurpose;

        var cacheEntryMoq = new Mock<ICacheEntry>() { CallBase = true };
        cacheEntryMoq.SetupSet(x => x.Value = string.Empty).Verifiable();
        cacheEntryMoq.SetupSet(x => x.AbsoluteExpirationRelativeToNow = _config.Duration).Verifiable();

        var memCacheMoq = new Mock<IMemoryCache>() { CallBase = true };
        memCacheMoq.Setup(p => p.CreateEntry($"Totp:{purpose}:1")).Returns(cacheEntryMoq.Object).Verifiable();

        var provider = new TotpTokenProvider(_config, TimeProvider.System, memCacheMoq.Object);
        var user = new User { Id = 1, SecurityStamp = Guid.NewGuid().ToString() };

        // Act
        provider.Generate(user, purpose);

        // Assert
        memCacheMoq.VerifyAll();
        cacheEntryMoq.VerifyAll();
    }

    [Fact]
    public void Validate_returns_false_for_non_numerical_otp()
    {
        // Arrange
        var purpose = TotpPurpose.SignInPurpose;
        var provider = new TotpTokenProvider(_config, TimeProvider.System, Mock.Of<IMemoryCache>());
        var user = new User { Id = 1, SecurityStamp = Guid.NewGuid().ToString() };

        // Act
        var result = provider.Validate("invalid", user, purpose);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsStillValid_checks_cache_to_determine_result()
    {
        // Arrange
        var purpose = TotpPurpose.SignInPurpose;

        var memCacheMoq = new Mock<IMemoryCache>(MockBehavior.Strict);
        memCacheMoq.Setup(p => p.TryGetValue($"Totp:{purpose}:1", out It.Ref<object>.IsAny)).Returns(false).Verifiable();

        var provider = new TotpTokenProvider(_config, TimeProvider.System, memCacheMoq.Object);
        var user = new User { Id = 1, SecurityStamp = Guid.NewGuid().ToString() };

        // Act
        provider.IsStillValid(user, purpose);

        // Assert
        memCacheMoq.VerifyAll();
    }

    [Fact]
    public void Generate_generates_defaultOtp_when_configured()
    {
        // Arrange
        var provider = new TotpTokenProvider(_defaultOtpConfig, TimeProvider.System, new MemoryCache(new MemoryCacheOptions()));
        var user = new User { Id = 1, SecurityStamp = Guid.NewGuid().ToString() };

        // Act
        var code = provider.Generate(user, TotpPurpose.SignInPurpose);

        // Assert
        Assert.Equal("1234", code);
    }

    [Fact]
    public void Validate_returns_true_for_defaultOtp_when_configured()
    {
        // Arrange
        var provider = new TotpTokenProvider(_defaultOtpConfig, TimeProvider.System, new MemoryCache(new MemoryCacheOptions()));
        var user = new User { Id = 1, SecurityStamp = Guid.NewGuid().ToString() };

        // Act
        var isValid = provider.Validate("1234", user, TotpPurpose.SignInPurpose);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void Validate_returns_false_for_invalid_defaultOtp()
    {
        // Arrange
        var provider = new TotpTokenProvider(_defaultOtpConfig, TimeProvider.System, new MemoryCache(new MemoryCacheOptions()));
        var user = new User { Id = 1, SecurityStamp = Guid.NewGuid().ToString() };

        // Act
        var isValid = provider.Validate("5678", user, TotpPurpose.SignInPurpose);

        // Assert
        Assert.False(isValid);
    }

    private class User : IdentityUserBase { }
}
