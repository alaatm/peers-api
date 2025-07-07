using System.Diagnostics.CodeAnalysis;

namespace Mashkoor.Core.Security.Hashing;

public interface IHmacHash
{
    /// <summary>
    /// Generates a new HMAC SHA256 key.
    /// </summary>
    /// <returns></returns>
    string GenerateKey();

    /// <summary>
    /// Verifies if the given input matches the provided HMAC SHA256 signature using the specified key.
    /// </summary>
    /// <param name="input">The input string to verify.</param>
    /// <param name="signature">The HMAC SHA256 signature to compare against.</param>
    /// <param name="key">The key used for hashing.</param>
    /// <returns></returns>
    bool IsValidSignature([NotNull] string input, [NotNull] string signature, [NotNull] string key);
}
