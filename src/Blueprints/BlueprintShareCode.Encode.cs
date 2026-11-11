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
        public static string Encode(string json)
        {
            byte[] raw = Utf8.GetBytes(json);
            if (raw.Length > MaxBytes) throw new InvalidDataException("The blueprint exceeds 8 MB.");
            using (var output = new MemoryStream())
            {
                using (var gzip = new GZipStream(output, CompressionLevel.Optimal, true)) gzip.Write(raw, 0, raw.Length);
                return Prefix + Convert.ToBase64String(output.ToArray());
            }
        }
    }
}
