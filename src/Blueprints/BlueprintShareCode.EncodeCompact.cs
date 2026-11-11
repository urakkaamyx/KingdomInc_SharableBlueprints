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
        public static string EncodeCompact(string json)
        {
            var document = PortableBlueprint.Parse(json).Document;
            byte[] packed = BlueprintCompactCode.Pack(document);
            using (var output = new MemoryStream())
            {
                using (var gzip = new GZipStream(output, CompressionLevel.Optimal, true)) gzip.Write(packed, 0, packed.Length);
                return CompactPrefix + BlueprintBase85.Encode(output.ToArray());
            }
        }
    }
}
