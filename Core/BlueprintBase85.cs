using System;
using System.IO;
using System.Text;

namespace SharableBlueprints
{
    public static class BlueprintBase85
    {
        const string Alphabet = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ.-:+=^!/*?&<>()[]{}@%$#";
        public static string Encode(byte[] bytes)
        {
            int padding = (4 - bytes.Length % 4) % 4;
            var text = new StringBuilder(1 + ((bytes.Length + padding) / 4) * 5); text.Append((char)('0' + padding));
            for (int offset = 0; offset < bytes.Length; offset += 4)
            {
                uint n = 0;
                for (int i = 0; i < 4; i++) n = (n << 8) | (offset + i < bytes.Length ? bytes[offset + i] : 0U);
                var block = new char[5];
                for (int i = 4; i >= 0; i--) { block[i] = Alphabet[(int)(n % 85)]; n /= 85; }
                text.Append(block);
            }
            return text.ToString();
        }
        public static byte[] Decode(string text)
        {
            if (text.Length < 6 || text[0] < '0' || text[0] > '3' || (text.Length - 1) % 5 != 0) throw new InvalidDataException("Invalid compact blueprint text.");
            int padding = text[0] - '0'; var bytes = new byte[((text.Length - 1) / 5) * 4]; int offset = 0;
            for (int start = 1; start < text.Length; start += 5)
            {
                ulong n = 0;
                for (int i = 0; i < 5; i++)
                {
                    int digit = Alphabet.IndexOf(text[start + i]); if (digit < 0) throw new InvalidDataException("Invalid compact blueprint character.");
                    n = n * 85 + (uint)digit;
                }
                if (n > uint.MaxValue) throw new InvalidDataException("Invalid compact blueprint block.");
                for (int i = 3; i >= 0; i--) { bytes[offset + i] = (byte)n; n >>= 8; }
                offset += 4;
            }
            for (int i = 0; i < padding; i++) if (bytes[bytes.Length - 1 - i] != 0) throw new InvalidDataException("Invalid compact blueprint padding.");
            Array.Resize(ref bytes, bytes.Length - padding); return bytes;
        }
    }
}
