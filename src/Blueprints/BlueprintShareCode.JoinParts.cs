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
        static string JoinParts(string text)
        {
            var matches = Regex.Matches(text, @"KIBPART/2:([0-9a-f]{12}):(\d+):(\d+):([^\s`]+)");
            if (matches.Count == 0) throw new InvalidDataException("No blueprint message parts found.");
            string key = matches[0].Groups[1].Value;
            if (!int.TryParse(matches[0].Groups[3].Value, out int count) || count <= 0 || count > 100000) throw new InvalidDataException("Invalid blueprint part count.");
            var parts = new Dictionary<int, string>();
            foreach (Match match in matches)
            {
                if (match.Groups[1].Value != key || match.Groups[3].Value != count.ToString()) throw new InvalidDataException("These message parts belong to different blueprints.");
                if (!int.TryParse(match.Groups[2].Value, out int index) || index < 1 || index > count || parts.ContainsKey(index)) throw new InvalidDataException("A blueprint part is repeated or invalid.");
                parts.Add(index, match.Groups[4].Value);
            }
            if (parts.Count != count) throw new InvalidDataException("Missing blueprint message parts: received " + parts.Count + " of " + count + ". Paste all parts together.");
            string code = string.Concat(Enumerable.Range(1, count).Select(i => parts[i]));
            if (PartKey(code) != key) throw new InvalidDataException("The blueprint message parts are damaged.");
            return code;
        }
    }
}
