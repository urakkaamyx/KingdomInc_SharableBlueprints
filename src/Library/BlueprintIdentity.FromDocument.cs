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
        public static BlueprintSuggestions FromDocument(JsonValue document)
        {
            var labels = new List<string>(); var icons = new List<BlueprintIconChoice>();
            foreach (var value in Array(document["nameOptions"])) if (value.IsString) labels.Add(value.AsString);
            foreach (var value in Array(document["iconOptions"]))
                if (value.IsJsonObject) icons.Add(new BlueprintIconChoice { Id = value["icon"].AsString, Name = value["name"].AsString });
            // Earlier captures already contain labels/stickers in their layout. Restore suggestions without changing files.
            var catalog = Catalog().ToDictionary(i => i.Id, i => i.Name, StringComparer.Ordinal);
            JsonValue layout = Field(document, "layout");
            foreach (var deco in Array(Field(layout, "Decorations")))
            {
                if (Field(deco, "Label").IsString) labels.Add(deco["Label"].AsString);
                JsonValue stickerIcon = Field(Field(deco, "Sticker"), "IconName");
                if (stickerIcon.IsString) icons.Add(Choice(stickerIcon.AsString, catalog));
            }
            var types = FlowElement.GetAllTypes().ToDictionary(e => e.ElementType, StringComparer.Ordinal);
            foreach (var element in Array(Field(layout, "Elements")))
                if (Field(element, "ElementType").IsString && types.TryGetValue(element["ElementType"].AsString, out var type))
                    icons.Add(new BlueprintIconChoice { Id = type.IconName, Name = Translation.Translate(type.Label) });
            return BlueprintSuggestions.Create(labels, icons);
        }
    }
}
