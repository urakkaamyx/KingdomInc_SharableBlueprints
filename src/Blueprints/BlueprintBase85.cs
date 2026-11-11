using System;
using System.IO;
using System.Text;

namespace SharableBlueprints
{
    public static partial class BlueprintBase85
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
    }
}
