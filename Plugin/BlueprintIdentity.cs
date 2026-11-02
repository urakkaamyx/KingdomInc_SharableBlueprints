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
    public static class BlueprintIdentity
    {
        public static BlueprintSuggestions FromSelection(FlowDiagram flow, SelectionBox bounds)
        {
            var decorations = SelectionContents.Decorations(flow, bounds).OrderByDescending(d => d.Position.y).ThenBy(d => d.Position.x).ToList();
            var catalog = Catalog().ToDictionary(i => i.Id, i => i.Name, StringComparer.Ordinal);
            var icons = new List<BlueprintIconChoice>();
            foreach (var deco in decorations)
                if (deco.Sticker is IconSticker sticker) icons.Add(Choice(sticker.IconName, catalog));
            foreach (var building in SelectionContents.Buildings(flow, bounds).OrderByDescending(e => e.Position.y).ThenBy(e => e.Position.x))
                icons.Add(new BlueprintIconChoice { Id = building.IconName, Name = Translation.Translate(building.Label) });
            return BlueprintSuggestions.Create(decorations.Select(d => d.Label), icons);
        }
        public static List<BlueprintIconChoice> Catalog()
            => BlueprintSuggestions.Create(null, SpriteLibrary.GetUsableIconNames().Select(i => new BlueprintIconChoice { Id = i.icon, Name = i.name })).Icons;
        static BlueprintIconChoice Choice(string id, IDictionary<string, string> catalog)
            => new BlueprintIconChoice { Id = id, Name = id != null && catalog.TryGetValue(id, out var name) ? name : id };
        public static Sprite Sprite(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            try { return SpriteLibrary.Get(id); } catch (KeyNotFoundException) { return null; }
        }
        public static void WriteSuggestions(JsonObject document, BlueprintSuggestions suggestions)
        {
            document["nameOptions"] = new JsonArray(suggestions.Labels.Select(s => (JsonValue)s).ToArray());
            document["iconOptions"] = new JsonArray(suggestions.Icons.Select(i => (JsonValue)new JsonObject { { "icon", i.Id }, { "name", i.Name } }).ToArray());
        }
        public static void Hydrate(LibraryBlueprint entry, string path)
        {
            if (entry.Suggestions.Labels.Count != 0 || entry.Suggestions.Icons.Count != 0 || !File.Exists(path)) return;
            JsonValue document = JsonValue.Parse(File.ReadAllText(path));
            if (!document.IsJsonObject) return;
            entry.Suggestions = FromDocument(document);
            if (string.IsNullOrEmpty(entry.IconName) && document["icon"].IsString) entry.IconName = document["icon"].AsString;
        }
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
        static IEnumerable<JsonValue> Array(JsonValue value) => value.IsJsonArray ? (JsonArray)value.AsObject : Enumerable.Empty<JsonValue>();
        static JsonValue Field(JsonValue value, string key) => value.IsJsonObject ? value[key] : JsonValue.Null;
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
