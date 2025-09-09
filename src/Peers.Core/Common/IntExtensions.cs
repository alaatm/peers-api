namespace Peers.Core.Common;

public static class IntExtensions
{
    extension(int value)
    {
        /// <summary>
        /// Encodes the integer to a base36 string, padded to at least 6 characters with leading zeros.
        /// </summary>
        /// <returns></returns>
        public string EncodeBase36()
        {
            // enough for int
            const int MaxLength = 13;
            const string Digits = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            Span<char> buf = stackalloc char[MaxLength];
            var i = MaxLength;
            var v = (uint)value;

            do
            {
                buf[--i] = Digits[(int)(v % 36)];
                v /= 36;
            } while (v > 0);

            var result = new string(buf[i..]);
            return result.PadLeft(6, '0');
        }
    }
}
