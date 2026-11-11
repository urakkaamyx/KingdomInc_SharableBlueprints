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
        static IEnumerable<JsonValue> Array(JsonValue value) => value.IsJsonArray ? (JsonArray)value.AsObject : Enumerable.Empty<JsonValue>();
    }
}
