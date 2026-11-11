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
        public static string Decode(string text)
        {
            if (text == null || text.Length > MaxCode + 65536) throw new InvalidDataException("The blueprint share code is too large.");
            text = text.Trim();
            if (text.Contains("KIBPART/2:")) text = JoinParts(text);
            if (text.StartsWith("```", StringComparison.Ordinal) && text.EndsWith("```", StringComparison.Ordinal) && text.Length >= 6)
            {
                text = text.Substring(3, text.Length - 6).Trim();
                int newline = text.IndexOf('\n');
                if (newline >= 0)
                {
                    string language = text.Substring(0, newline).Trim().ToLowerInvariant();
                    if (language == "text" || language == "txt" || language == "base64" || language == "json") text = text.Substring(newline + 1).Trim();
                }
            }
            text = text.Trim('`', ' ', '\r', '\n', '\t');
            if (text.StartsWith("{", StringComparison.Ordinal)) return text;
            bool compact = text.StartsWith(CompactPrefix, StringComparison.Ordinal);
            if (compact) text = text.Substring(CompactPrefix.Length);
            if (text.StartsWith(Prefix, StringComparison.Ordinal)) text = text.Substring(Prefix.Length);
            var code = new StringBuilder(text.Length);
            foreach (char c in text) if (!char.IsWhiteSpace(c)) code.Append(compact ? c : c == '-' ? '+' : c == '_' ? '/' : c);
            if (code.Length == 0 || code.Length > MaxCode) throw new InvalidDataException("Paste blueprint JSON or a Base64 share code.");
            if (!compact) while (code.Length % 4 != 0) code.Append('=');
            try
            {
                byte[] raw = compact ? BlueprintBase85.Decode(code.ToString()) : Convert.FromBase64String(code.ToString());
                if (raw.Length >= 2 && raw[0] == 0x1f && raw[1] == 0x8b)
                {
                    if (raw.Length < 18) throw new InvalidDataException("The compressed blueprint is incomplete.");
                    uint checksum = LittleEndian(raw, raw.Length - 8), length = LittleEndian(raw, raw.Length - 4);
                    using (var input = new MemoryStream(raw))
                    using (var gzip = new GZipStream(input, CompressionMode.Decompress))
                    using (var output = new MemoryStream())
                    {
                        var buffer = new byte[8192]; int count;
                        while ((count = gzip.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            if (output.Length + count > MaxBytes) throw new InvalidDataException("The expanded blueprint exceeds 8 MB.");
                            output.Write(buffer, 0, count);
                        }
                        raw = output.ToArray();
                    }
                    if (length != raw.Length || checksum != Crc32(raw))
                        throw new InvalidDataException("The compressed blueprint is incomplete or damaged.");
                }
                if (raw.Length > MaxBytes) throw new InvalidDataException("The blueprint exceeds 8 MB.");
                if (compact) return BlueprintCompactCode.Unpack(raw);
                string json = Utf8.GetString(raw).TrimStart('\uFEFF');
                if (!json.TrimStart().StartsWith("{", StringComparison.Ordinal)) throw new InvalidDataException("The share code does not contain blueprint JSON.");
                return json;
            }
            catch (FormatException error) { throw new InvalidDataException("This is not a valid Base64 blueprint code.", error); }
            catch (DecoderFallbackException error) { throw new InvalidDataException("The blueprint share code is not UTF-8 text.", error); }
        }
    }
}
