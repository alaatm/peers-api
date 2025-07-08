using System.Security.Cryptography;
using System.Text;

namespace Mashkoor.Core.Security.StrongKeys;

public static class KeyGenerator
{
    private static readonly char[] _chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
    private static readonly char[] _charsLoweCase = "abcdefghijklmnopqrstuvwxyz1234567890".ToCharArray();
    private static readonly char[] _nums = "1234567890".ToCharArray();

    /// <summary>
    /// Generates a random key that adheres to NIST SP800-90 standard.
    /// </summary>
    /// <param name="size">The size of they key.</param>
    /// <param name="numbersOnly">If true, the returned key will only contain digits; otherwise, it will be a mix of digits and characters.</param>
    /// <param name="lowerCaseOnly">If true, the returned key will only contain lower case letters and digits.</param>
    /// <returns></returns>
    public static string Create(int size, bool numbersOnly = false, bool lowerCaseOnly = false)
    {
        // If numbers only is set, lower case only is ignored.

        var data = RandomNumberGenerator.GetBytes(4 * size);
        var source = numbersOnly
            ? _nums
            : lowerCaseOnly
                ? _charsLoweCase
                : _chars;

        var result = new StringBuilder(size);
        for (var i = 0; i < size; i++)
        {
            var rnd = BitConverter.ToUInt32(data, i * 4);
            var idx = rnd % source.Length;

            result.Append(source[idx]);
        }

        return result.ToString();
    }
}
