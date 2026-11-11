using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LightJson;

namespace SharableBlueprints
{
    public sealed partial class PortableBlueprint
    {
        static string Endpoint(JsonValue endpoint, Dictionary<string, JsonValue> nodes, string ports)
        {
            string id = Field(endpoint, "Guid").AsString;
            var index = Field(endpoint, "Index");
            if (id == null || !nodes.TryGetValue(id, out var node) || !index.IsInteger || index.AsInteger < 0 || index.AsInteger >= node[ports].AsJsonArray.Count)
                throw new InvalidDataException("A connection points to a missing building or port.");
            return id + "/" + index.AsInteger;
        }
    }
}
