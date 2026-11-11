using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace SharableBlueprints
{
    public static partial class BlueprintShareCode
    {
        public static List<string> ForDiscord(string code, int budget = 1990)
        {
            if (budget < 100 || budget > 1992) throw new ArgumentOutOfRangeException(nameof(budget));
            if (code.Length <= budget) return new List<string> { code };
            string key = PartKey(code); int payloadSize = budget - 64, count = (code.Length + payloadSize - 1) / payloadSize;
            var result = new List<string>();
            for (int i = 0; i < count; i++) result.Add("KIBPART/2:" + key + ":" + (i + 1) + ":" + count + ":" + code.Substring(i * payloadSize, Math.Min(payloadSize, code.Length - i * payloadSize)));
            return result;
        }
    }
}
