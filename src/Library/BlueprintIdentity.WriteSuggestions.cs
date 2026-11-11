using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assets.Source.Flow;
using Assets.Source.Flow.Stickers;
using Assets.Source.Util;
using LightJson;
using UnityEngine;

namespace SharableBlueprints
{
    public static partial class BlueprintIdentity
    {
        public static void WriteSuggestions(JsonObject document, BlueprintSuggestions suggestions)
        {
            document["nameOptions"] = new JsonArray(suggestions.Labels.Select(s => (JsonValue)s).ToArray());
            document["iconOptions"] = new JsonArray(suggestions.Icons.Select(i => (JsonValue)new JsonObject { { "icon", i.Id }, { "name", i.Name } }).ToArray());
        }
    }
}
