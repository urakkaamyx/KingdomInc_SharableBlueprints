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
        static BlueprintIconChoice Choice(string id, IDictionary<string, string> catalog)
            => new BlueprintIconChoice { Id = id, Name = id != null && catalog.TryGetValue(id, out var name) ? name : id };
    }
}
