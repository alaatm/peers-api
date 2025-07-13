using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using Mashkoor.Core.Data;
using Mashkoor.Core.Security.Totp.Configuration;

namespace Mashkoor.Core.Security.Totp;

/// <summary>
/// A time-based one-time password provider.
/// </summary>
public abstract class TotpTokenProviderBase : ITotpTokenProvider
{
    private readonly TotpConfig _config;
    private readonly TimeProvider _timeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="TotpTokenProviderBase"/> class.
    /// </summary>
    /// <param name="config">The TOTP configuration.</param>
    /// <param name="timeProvider">The time provider to use for generating TOTP codes.</param>
    protected TotpTokenProviderBase([NotNull] TotpConfig config, TimeProvider timeProvider)
    {
        _config = config;
        _timeProvider = timeProvider;
    }

    /// <summary>
    /// Attempts to generate new code only if a valid one no longer or does not exist.
    /// </summary>
    /// <param name="user">The user to generated the OTP for.</param>
    /// <param name="purpose">The purpose of the OTP.</param>
    /// <param name="code">The generated code.</param>
    /// <returns></returns>
    public bool TryGenerate([NotNull] IdentityUserBase user, string purpose, [NotNullWhen(true)] out string? code)
    {
        code = null;
        if (IsStillValid(user, purpose))
        {
            return false;
        }

        code = Generate(user, purpose);
        return true;
    }

    /// <summary>
    /// Generates a time-based one-time password.
    /// </summary>
    /// <param name="user">The user to generated the OTP for.</param>
    /// <param name="purpose">The purpose of the OTP.</param>
    /// <returns></returns>
    public string Generate([NotNull] IdentityUserBase user, string purpose)
    {
        if (_config.UseDefaultOtp)
        {
            return _config.DefaultOtp;
        }

        var modifier = GetUserModifier(user, purpose);
        SetCacheValue(modifier, string.Empty, _config.Duration);

        return Rfc6238AuthenticationService
            .GenerateCode(_timeProvider, _config.Duration, CreateSecurityToken(user), modifier)
            .ToString("D4", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Validates the specified time-based one-time password.
    /// </summary>
    /// <param name="token">The OTP.</param>
    /// <param name="user">The user to check the OTP for.</param>
    /// <param name="purpose">The purpose of the OTP.</param>
    /// <returns></returns>
    public bool Validate(string token, [NotNull] IdentityUserBase user, string purpose)
    {
        if (!int.TryParse(token, out var code))
        {
            return false;
        }

        if (_config.UseDefaultOtp)
        {
            return token == _config.DefaultOtp;
        }

        var securityToken = CreateSecurityToken(user);
        var modifier = GetUserModifier(user, purpose);
        return Rfc6238AuthenticationService.ValidateCode(_timeProvider, _config.Duration, securityToken, code, modifier);
    }

    /// <summary>
    /// Returns whether previously generated OTP for the user should still be valid.
    /// </summary>
    /// <param name="user">The user to check prev generated OTP validity.</param>
    /// <param name="purpose">The purpose of the OTP.</param>
    /// <returns></returns>
    public bool IsStillValid([NotNull] IdentityUserBase user, string purpose)
        => TryGetCacheValue(GetUserModifier(user, purpose), out _);

    protected abstract void SetCacheValue(string key, string value, TimeSpan expiration);

    protected abstract bool TryGetCacheValue(string key, [NotNullWhen(true)] out object? value);

    private static string GetUserModifier(IdentityUserBase user, string purpose) => $"Totp:{purpose}:{user.Id}";

    private static byte[] CreateSecurityToken(IdentityUserBase user)
        => Encoding.Unicode.GetBytes(user.SecurityStamp!);
}
