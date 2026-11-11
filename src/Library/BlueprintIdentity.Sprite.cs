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
        public static Sprite Sprite(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            try { return SpriteLibrary.Get(id); } catch (KeyNotFoundException) { return null; }
        }
    }
}
