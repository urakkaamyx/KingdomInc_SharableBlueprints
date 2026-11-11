using System;
using System.IO;
using System.Text;

namespace SharableBlueprints
{
    public static partial class BlueprintBase85
    {
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
