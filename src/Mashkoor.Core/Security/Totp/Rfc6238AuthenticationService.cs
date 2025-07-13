using System.Diagnostics;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace Mashkoor.Core.Security.Totp;

#pragma warning disable CA5350 // Do Not Use Weak Cryptographic Algorithms
internal static class Rfc6238AuthenticationService
{
    private static readonly Encoding _encoding = new UTF8Encoding(false, true);

    private static int ComputeTotp(HashAlgorithm hashAlgorithm, ulong timestepNumber, string modifier)
    {
        // # of 0's = length of pin
        const int Mod = 10000;

        // See https://tools.ietf.org/html/rfc4226
        // We can add an optional modifier
        var timestepAsBytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((long)timestepNumber));
        var hash = hashAlgorithm.ComputeHash(ApplyModifier(timestepAsBytes, modifier));

        // Generate DT string
        var offset = hash[^1] & 0xf;
        Debug.Assert(offset + 4 < hash.Length);
        var binaryCode = ((hash[offset] & 0x7f) << 24)
                         | ((hash[offset + 1] & 0xff) << 16)
                         | ((hash[offset + 2] & 0xff) << 8)
                         | (hash[offset + 3] & 0xff);

        return binaryCode % Mod;
    }

    private static byte[] ApplyModifier(byte[] input, string modifier)
    {
        var modifierBytes = _encoding.GetBytes(modifier);
        var combined = new byte[checked(input.Length + modifierBytes.Length)];
        Buffer.BlockCopy(input, 0, combined, 0, input.Length);
        Buffer.BlockCopy(modifierBytes, 0, combined, input.Length, modifierBytes.Length);
        return combined;
    }

    // More info: https://tools.ietf.org/html/rfc6238#section-4
    private static ulong GetCurrentTimeStepNumber(TimeProvider timeProvider, TimeSpan timestep)
    {
        var delta = timeProvider.GetUtcNow() - DateTimeOffset.UnixEpoch;
        return (ulong)(delta.Ticks / timestep.Ticks);
    }

    public static int GenerateCode(TimeProvider timeProvider, TimeSpan timestep, byte[] securityToken, string modifier)
    {
        Debug.Assert(securityToken is not null);

        var currentTimeStep = GetCurrentTimeStepNumber(timeProvider, timestep);
        using var hashAlgorithm = new HMACSHA1(securityToken);
        return ComputeTotp(hashAlgorithm, currentTimeStep, modifier);
    }

    public static bool ValidateCode(TimeProvider timeProvider, TimeSpan timestep, byte[] securityToken, int code, string modifier)
    {
        Debug.Assert(securityToken is not null);

        // Allow a variance of 1 second on left and 0 secs on right.
        const int LeftAllowedVariance = -1;
        const int RightAllowedVariance = 0;

        var currentTimeStep = GetCurrentTimeStepNumber(timeProvider, timestep);
        using var hashAlgorithm = new HMACSHA1(securityToken);
        for (var i = LeftAllowedVariance; i <= RightAllowedVariance; i++)
        {
            var computedTotp = ComputeTotp(hashAlgorithm, (ulong)((long)currentTimeStep + i), modifier);
            if (computedTotp == code)
            {
                return true;
            }
        }

        // No match
        return false;
    }
}
#pragma warning restore CA5350 // Do Not Use Weak Cryptographic Algorithms
