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
        public static void Hydrate(LibraryBlueprint entry, string path)
        {
            if (entry.Suggestions.Labels.Count != 0 || entry.Suggestions.Icons.Count != 0 || !File.Exists(path)) return;
            JsonValue document = JsonValue.Parse(File.ReadAllText(path));
            if (!document.IsJsonObject) return;
            entry.Suggestions = FromDocument(document);
            if (string.IsNullOrEmpty(entry.IconName) && document["icon"].IsString) entry.IconName = document["icon"].AsString;
        }
    }
}
