using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LightJson;

namespace SharableBlueprints
{
    public static partial class BlueprintCompactCode
    {
        static JsonObject Except(JsonObject value, params string[] keys)
        {
            var result = new JsonObject();
            foreach (var pair in ((IEnumerable<KeyValuePair<string, JsonValue>>)value).OrderBy(p => p.Key, StringComparer.Ordinal)) if (!keys.Contains(pair.Key)) result[pair.Key] = pair.Value;
            return result;
        }
    }
}
