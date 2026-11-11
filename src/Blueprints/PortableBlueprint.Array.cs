using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LightJson;

namespace SharableBlueprints
{
    public sealed partial class PortableBlueprint
    {
        public static IEnumerable<JsonValue> Array(JsonValue value) => value.IsJsonArray ? value.AsJsonArray : Enumerable.Empty<JsonValue>();
    }
}
