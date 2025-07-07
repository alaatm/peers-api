using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Mashkoor.Core.Common.Configs;
using Mashkoor.Core.Identity;

namespace Mashkoor.Core.Security.Jwt;

/// <summary>
/// The JWT configuration.
/// </summary>
public sealed class JwtConfig : IConfigSection
{
    public const string ConfigSection = "jwt";
    static string IConfigSection.ConfigSection => ConfigSection;

    /// <summary>
    /// The JWT issuer.
    /// </summary>
    public string Issuer { get; set; } = default!;
    /// <summary>
    /// The JWT key.
    /// </summary>
    public string Key { get; set; } = default!;
    /// <summary>
    /// The duration of issued JWTs in minutes.
    /// </summary>
    public int DurationInMinutes { get; set; }

    // Note below prop can never return null but is marked as allow null to get rid of compiler warning which thinks it can be null (it can't).
    [field: AllowNull] public byte[] KeyBytes => field ??= Convert.FromBase64String(Key);

    /// <summary>
    /// Returns JWT validation parameters.
    /// </summary>
    public TokenValidationParameters TokenValidationParameters => new()
    {
        // For these 2 to work, we need to set MapInboundClaims to false in the JwtBearerOptions in AddJwtBearer call
        NameClaimType = CustomClaimTypes.Username,
        RoleClaimType = CustomClaimTypes.Role,

        ValidIssuer = Issuer,
        ValidateIssuer = true,

        // Our API is the only issuer and consumer of the JWT, so we can disable Audience check
        // as we only need to validate the issuer
#pragma warning disable CA5404
        ValidAudience = null,
        ValidateAudience = false,
        AudienceValidator = null,
#pragma warning restore CA5404 // Do not disable token validation checks

        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(KeyBytes),
    };
}

internal sealed class JwtConfigValidator : IValidateOptions<JwtConfig>
{
    public ValidateOptionsResult Validate(string? name, JwtConfig options)
    {
        if (options.Issuer is null || !RegexStatic.UriRegex().IsMatch(options.Issuer))
        {
            return ValidateOptionsResult.Fail($"{JwtConfig.ConfigSection}:{nameof(JwtConfig.Issuer)} must be a valid URI.");
        }
        if (options.Key is null || !RegexStatic.Base64Regex().IsMatch(options.Key))
        {
            return ValidateOptionsResult.Fail($"{JwtConfig.ConfigSection}:{nameof(JwtConfig.Key)} must be a base64 encoded string.");
        }
        if (options.DurationInMinutes < 10)
        {
            return ValidateOptionsResult.Fail($"{JwtConfig.ConfigSection}:{nameof(JwtConfig.DurationInMinutes)} must be set to 10 or greater.");
        }

        return ValidateOptionsResult.Success;
    }
}
