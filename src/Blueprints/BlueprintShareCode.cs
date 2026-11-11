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
        public const string Prefix = "KIBLUEPRINT/1:";
        public const string CompactPrefix = "KIB2:";
        const int MaxBytes = 8 * 1024 * 1024;
        const int MaxCode = ((MaxBytes + 2) / 3) * 4 + 100;
        static readonly Encoding Utf8 = new UTF8Encoding(false, true);

        static uint Crc32(byte[] bytes)
        {
            uint crc = 0xffffffff;
            foreach (byte b in bytes)
            {
                crc ^= b;
                for (int i = 0; i < 8; i++) crc = (crc >> 1) ^ ((crc & 1) != 0 ? 0xedb88320U : 0U);
            }
            return ~crc;
        }
    }
}
