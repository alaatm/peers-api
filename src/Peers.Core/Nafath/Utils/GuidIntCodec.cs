namespace Peers.Core.Nafath.Utils;

/// <summary>
/// Provides methods for encoding an integer value into a deterministic, reversible GUID and decoding the original
/// integer from such a GUID.
/// </summary>
internal static class GuidIntCodec
{
    /// <summary>
    /// Encodes an integer value into a deterministic, reversible GUID.
    /// </summary>
    /// <param name="value">The value to encode.</param>
    public static Guid Encode(int value)
    {
        Span<byte> bytes = stackalloc byte[16];

        // Store the int in the first 4 bytes (little-endian).
        BitConverter.GetBytes(value).CopyTo(bytes);

        // Fill the remaining 12 bytes with a deterministic pseudo-random
        // sequence based on the input value so the GUID looks random.
        var state = (uint)value;

        // Simple xorshift32 PRNG seeded by the input value
        for (var i = 4; i < 16; i++)
        {
            state ^= state << 13;
            state ^= state >> 17;
            state ^= state << 5;
            bytes[i] = (byte)state;
        }

        // Set version and variant bits to make it look like a standard UUIDv4
        // Version: high 4 bits of byte 7
        bytes[7] = (byte)((bytes[7] & 0x0f) | 0x40);
        // Variant: high 2 bits of byte 8
        bytes[8] = (byte)((bytes[8] & 0x3f) | 0x80);

        return new Guid(bytes);
    }

    /// <summary>
    /// Decodes the original integer value from a GUID created by <see cref="Encode(int)"/>.
    /// </summary>
    /// <param name="value">The value to decode.</param>
    public static int Decode(Guid value)
    {
        Span<byte> bytes = stackalloc byte[16];
        value.TryWriteBytes(bytes);
        return BitConverter.ToInt32(bytes);
    }
}