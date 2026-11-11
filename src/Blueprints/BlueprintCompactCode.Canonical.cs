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
        static string Canonical(JsonValue value)
        {
            if (value.IsJsonObject)
            {
                var sorted = new JsonObject();
                foreach (var pair in ((IEnumerable<KeyValuePair<string, JsonValue>>)value.AsJsonObject).OrderBy(p => p.Key, StringComparer.Ordinal)) sorted[pair.Key] = JsonValue.Parse(Canonical(pair.Value));
                return sorted.ToString(false);
            }
            if (value.IsJsonArray) return new JsonArray(value.AsJsonArray.Select(v => JsonValue.Parse(Canonical(v))).ToArray()).ToString(false);
            return value.ToString(false);
        }
    }
}
