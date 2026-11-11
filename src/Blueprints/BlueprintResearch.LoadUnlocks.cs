using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Assets.Source.Flow;
using Assets.Source.Items;
using Assets.Source.Player;
using Assets.Source.Util;
using LightJson;

namespace SharableBlueprints
{
    public static partial class BlueprintResearch
    {
        static Dictionary<string, List<string>> LoadUnlocks()
        {
            var result = new Dictionary<string, List<string>>(StringComparer.Ordinal);
            using (var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("research-unlocks.tsv")))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.StartsWith("#") || line.Length == 0) continue;
                    var parts = line.Split('\t'); if (parts.Length != 3) continue;
                    string key = parts[0] + "/" + parts[1];
                    if (!result.TryGetValue(key, out var ids)) result.Add(key, ids = new List<string>());
                    ids.Add(parts[2]);
                }
            }
            return result;
        }
    }
}
