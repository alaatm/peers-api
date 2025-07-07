using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Mashkoor.Core.Security.Hashing;

public sealed class HmacHash : IHmacHash
{
    /// <summary>
    /// Generates a new HMAC SHA256 key.
    /// </summary>
    /// <returns></returns>
    public string GenerateKey()
        => Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(32));

    /// <summary>
    /// Verifies if the given input matches the provided HMAC SHA256 signature using the specified key.
    /// </summary>
    /// <param name="input">The input string to verify.</param>
    /// <param name="signature">The HMAC SHA256 signature to compare against.</param>
    /// <param name="key">The key used for hashing.</param>
    /// <returns></returns>
    public bool IsValidSignature(
        [NotNull] string input,
        [NotNull] string signature,
        [NotNull] string key)
    {
        using var hmac = new HMACSHA256(Base64UrlEncoder.DecodeBytes(key));
        var computedBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(input));

        Span<byte> incomingBytes = stackalloc byte[computedBytes.Length];
        if (Convert.FromHexString(signature, incomingBytes, out var charsConsumed, out var bytesWritten) is not OperationStatus.Done ||
            charsConsumed != signature.Length ||
            bytesWritten != computedBytes.Length)
        {
            // Bad hex, wrong length, or extra characters
            return false;
        }

        // Constant-time compare to prevents timing attacks
        return CryptographicOperations.FixedTimeEquals(computedBytes, incomingBytes);
    }
}
