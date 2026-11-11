using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LightJson;

namespace SharableBlueprints
{
    public sealed partial class PortableBlueprint
    {
        public JsonObject Document;
        public string Name, Icon;
        public int Buildings, Pipes, Decorations;
        public static JsonValue Field(JsonValue value, string key) => value.IsJsonObject ? value[key] : JsonValue.Null;
    }
}
