using System.Diagnostics.CodeAnalysis;
using Mashkoor.Core.Data;

namespace Mashkoor.Core.Security.Totp;

public interface ITotpTokenProvider
{
    /// <summary>
    /// Attempts to generate new code only if a valid one no longer or does not exist.
    /// </summary>
    /// <param name="user">The user to generated the OTP for.</param>
    /// <param name="purpose">The purpose of the OTP.</param>
    /// <param name="code">The generated code.</param>
    /// <returns></returns>
    bool TryGenerate([NotNull] IdentityUserBase user, string purpose, [NotNullWhen(true)] out string? code);
    /// <summary>
    /// Generates a time-based one-time password.
    /// </summary>
    /// <param name="user">The user to generated the OTP for.</param>
    /// <param name="purpose">The purpose of the OTP.</param>
    /// <returns></returns>
    string Generate([NotNull] IdentityUserBase user, string purpose);
    /// <summary>
    /// Validates the specified time-based one-time password.
    /// </summary>
    /// <param name="token">The OTP.</param>
    /// <param name="user">The user to check the OTP for.</param>
    /// <param name="purpose">The purpose of the OTP.</param>
    /// <returns></returns>
    bool Validate(string token, [NotNull] IdentityUserBase user, string purpose);
    /// <summary>
    /// Returns whether previously generated OTP for the user should still be valid.
    /// </summary>
    /// <param name="user">The user to check prev generated OTP validity.</param>
    /// <param name="purpose">The purpose of the OTP.</param>
    /// <returns></returns>
    bool IsStillValid([NotNull] IdentityUserBase user, string purpose);
}
