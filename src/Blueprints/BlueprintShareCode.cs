using KingdomFoundation.Api;
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
    public static class BlueprintShareCode
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
                for (int i = 0; i < 8; i++)
                    crc = (crc >> 1) ^ ((crc & 1) != 0 ? 0xedb88320U : 0U);
            }

            return ~crc;
        }

        public static string Decode(string text)
        {
            if (text == null || text.Length > MaxCode + 65536)
                throw new InvalidDataException("The blueprint share code is too large.");
            text = text.Trim();
            if (text.Contains("KIBPART/2:"))
                text = JoinParts(text);
            if (text.StartsWith("```", StringComparison.Ordinal) && text.EndsWith("```", StringComparison.Ordinal) && text.Length >= 6)
            {
                text = text.Substring(3, text.Length - 6).Trim();
                int newline = text.IndexOf('\n');
                if (newline >= 0)
                {
                    string language = text.Substring(0, newline).Trim().ToLowerInvariant();
                    if (language == "text" || language == "txt" || language == "base64" || language == "json")
                        text = text.Substring(newline + 1).Trim();
                }
            }

            text = text.Trim('`', ' ', '\r', '\n', '\t');
            if (text.StartsWith("{", StringComparison.Ordinal))
                return text;
            bool compact = text.StartsWith(CompactPrefix, StringComparison.Ordinal);
            if (compact)
                text = text.Substring(CompactPrefix.Length);
            if (text.StartsWith(Prefix, StringComparison.Ordinal))
                text = text.Substring(Prefix.Length);
            var code = new StringBuilder(text.Length);
            foreach (char c in text)
                if (!char.IsWhiteSpace(c))
                    code.Append(compact ? c : c == '-' ? '+' : c == '_' ? '/' : c);
            if (code.Length == 0 || code.Length > MaxCode)
                throw new InvalidDataException("Paste blueprint JSON or a Base64 share code.");
            if (!compact)
                while (code.Length % 4 != 0)
                    code.Append('=');
            try
            {
                byte[] raw = compact ? BlueprintBase85.Decode(code.ToString()) : Convert.FromBase64String(code.ToString());
                if (raw.Length >= 2 && raw[0] == 0x1f && raw[1] == 0x8b)
                {
                    if (raw.Length < 18)
                        throw new InvalidDataException("The compressed blueprint is incomplete.");
                    uint checksum = LittleEndian(raw, raw.Length - 8), length = LittleEndian(raw, raw.Length - 4);
                    using (var input = new MemoryStream(raw))
                    using (var gzip = new GZipStream(input, CompressionMode.Decompress))
                    using (var output = new MemoryStream())
                    {
                        var buffer = new byte[8192];
                        int count;
                        while ((count = gzip.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            if (output.Length + count > MaxBytes)
                                throw new InvalidDataException("The expanded blueprint exceeds 8 MB.");
                            output.Write(buffer, 0, count);
                        }

                        raw = output.ToArray();
                    }

                    if (length != raw.Length || checksum != Crc32(raw))
                        throw new InvalidDataException("The compressed blueprint is incomplete or damaged.");
                }

                if (raw.Length > MaxBytes)
                    throw new InvalidDataException("The blueprint exceeds 8 MB.");
                if (compact)
                    return BlueprintCompactCode.Unpack(raw);
                string json = Utf8.GetString(raw).TrimStart('\uFEFF');
                if (!json.TrimStart().StartsWith("{", StringComparison.Ordinal))
                    throw new InvalidDataException("The share code does not contain blueprint JSON.");
                return json;
            }
            catch (FormatException error)
            {
                throw new InvalidDataException("This is not a valid Base64 blueprint code.", error);
            }
            catch (DecoderFallbackException error)
            {
                throw new InvalidDataException("The blueprint share code is not UTF-8 text.", error);
            }
        }

        public static string Encode(string json)
        {
            byte[] raw = Utf8.GetBytes(json);
            if (raw.Length > MaxBytes)
                throw new InvalidDataException("The blueprint exceeds 8 MB.");
            using (var output = new MemoryStream())
            {
                using (var gzip = new GZipStream(output, CompressionLevel.Optimal, true))
                    gzip.Write(raw, 0, raw.Length);
                return Prefix + Convert.ToBase64String(output.ToArray());
            }
        }

        public static string EncodeCompact(string json)
        {
            var document = PortableBlueprint.Parse(json).Document;
            byte[] packed = BlueprintCompactCode.Pack(document);
            using (var output = new MemoryStream())
            {
                using (var gzip = new GZipStream(output, CompressionLevel.Optimal, true))
                    gzip.Write(packed, 0, packed.Length);
                return CompactPrefix + BlueprintBase85.Encode(output.ToArray());
            }
        }

        public static List<string> ForDiscord(string code, int budget = 1990)
        {
            if (budget < 100 || budget > 1992)
                throw new ArgumentOutOfRangeException(nameof(budget));
            if (code.Length <= budget)
                return new List<string>
                {
                    code
                };
            string key = PartKey(code);
            int payloadSize = budget - 64, count = (code.Length + payloadSize - 1) / payloadSize;
            var result = new List<string>();
            for (int i = 0; i < count; i++)
                result.Add("KIBPART/2:" + key + ":" + (i + 1) + ":" + count + ":" + code.Substring(i * payloadSize, Math.Min(payloadSize, code.Length - i * payloadSize)));
            return result;
        }

        static string JoinParts(string text)
        {
            var matches = Regex.Matches(text, @"KIBPART/2:([0-9a-f]{12}):(\d+):(\d+):([^\s`]+)");
            if (matches.Count == 0)
                throw new InvalidDataException("No blueprint message parts found.");
            string key = matches[0].Groups[1].Value;
            if (!int.TryParse(matches[0].Groups[3].Value, out int count) || count <= 0 || count > 100000)
                throw new InvalidDataException("Invalid blueprint part count.");
            var parts = new Dictionary<int, string>();
            foreach (Match match in matches)
            {
                if (match.Groups[1].Value != key || match.Groups[3].Value != count.ToString())
                    throw new InvalidDataException("These message parts belong to different blueprints.");
                if (!int.TryParse(match.Groups[2].Value, out int index) || index < 1 || index > count || parts.ContainsKey(index))
                    throw new InvalidDataException("A blueprint part is repeated or invalid.");
                parts.Add(index, match.Groups[4].Value);
            }

            if (parts.Count != count)
                throw new InvalidDataException("Missing blueprint message parts: received " + parts.Count + " of " + count + ". Paste all parts together.");
            string code = string.Concat(Enumerable.Range(1, count).Select(i => parts[i]));
            if (PartKey(code) != key)
                throw new InvalidDataException("The blueprint message parts are damaged.");
            return code;
        }

        static uint LittleEndian(byte[] bytes, int offset) => (uint)bytes[offset] | ((uint)bytes[offset + 1] << 8) | ((uint)bytes[offset + 2] << 16) | ((uint)bytes[offset + 3] << 24);
        static string PartKey(string code)
        {
            using (var hash = SHA256.Create())
                return BitConverter.ToString(hash.ComputeHash(Utf8.GetBytes(code))).Replace("-", "").Substring(0, 12).ToLowerInvariant();
        }
    }
}
