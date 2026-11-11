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
        public static List<BlueprintIconChoice> Catalog()
            => BlueprintSuggestions.Create(null, SpriteLibrary.GetUsableIconNames().Select(i => new BlueprintIconChoice { Id = i.icon, Name = i.name })).Icons;
    }
}
