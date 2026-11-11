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
        static string PartKey(string code)
        {
            using (var hash = SHA256.Create()) return BitConverter.ToString(hash.ComputeHash(Utf8.GetBytes(code))).Replace("-", "").Substring(0, 12).ToLowerInvariant();
        }
    }
}
