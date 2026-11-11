using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LightJson;

namespace SharableBlueprints
{
    public sealed partial class PortableBlueprint
    {
        public static IEnumerable<(string key, string value)> References(JsonValue data, string key = "")
        {
            if (data.IsString)
            {
                if (key != "Guid" && key != "ElementType" && key != "IconName" && key != "Label" && !string.IsNullOrEmpty(data.AsString)) yield return (key, data.AsString);
            }
            else if (data.IsJsonObject)
                foreach (var pair in data.AsJsonObject) foreach (var reference in References(pair.Value, pair.Key)) yield return reference;
            else if (data.IsJsonArray)
                foreach (var child in data.AsJsonArray) foreach (var reference in References(child, key)) yield return reference;
        }
    }
}
