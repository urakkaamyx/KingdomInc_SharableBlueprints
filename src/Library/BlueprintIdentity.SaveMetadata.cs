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
        public static void SaveMetadata(BlueprintLibraryStore store, LibraryBlueprint entry, string name, string icon)
        {
            name = BlueprintLibraryStore.Name(name);
            JsonValue value = JsonValue.Parse(File.ReadAllText(store.FilePath(entry)));
            if (!value.IsJsonObject || value["format"].AsString != "kingdominc.sharable-blueprint")
                throw new InvalidDataException("This blueprint file cannot be edited. Its contents have been preserved.");
            var document = (JsonObject)value.AsObject;
            document["name"] = name; document["icon"] = icon ?? ""; WriteSuggestions(document, entry.Suggestions);
            store.UpdateMetadata(entry.Id, name, icon, document.ToString(true));
        }
    }
}
