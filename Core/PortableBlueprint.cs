using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LightJson;

namespace SharableBlueprints
{
    public sealed class PortableBlueprint
    {
        public JsonObject Document;
        public string Name, Icon;
        public int Buildings, Pipes, Decorations;
        public static JsonValue Field(JsonValue value, string key) => value.IsJsonObject ? value[key] : JsonValue.Null;
        public static IEnumerable<JsonValue> Array(JsonValue value) => value.IsJsonArray ? value.AsJsonArray : Enumerable.Empty<JsonValue>();
        public static PortableBlueprint Parse(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) throw new InvalidDataException("Paste a blueprint share code or JSON first.");
            text = BlueprintShareCode.Decode(text);
            if (text.Length > 8 * 1024 * 1024) throw new InvalidDataException("The blueprint is too large (maximum 8 MB of text).");
            JsonValue value;
            try { value = JsonValue.Parse(text); } catch { throw new InvalidDataException("This text is not valid blueprint JSON."); }
            if (!value.IsJsonObject || Field(value, "format").AsString != "kingdominc.sharable-blueprint" || !Field(value, "formatVersion").IsInteger || (int)value["formatVersion"] != 1)
                throw new InvalidDataException("Unsupported blueprint format or version.");
            var layout = Field(value, "layout");
            var elements = Field(layout, "Elements"); var pipes = Field(layout, "Pipes"); var decos = Field(layout, "Decorations");
            if (!elements.IsJsonArray || elements.AsJsonArray.Count == 0 || !pipes.IsJsonArray || !decos.IsJsonArray)
                throw new InvalidDataException("The blueprint must contain buildings, pipes and decorations lists.");
            var nodes = new Dictionary<string, JsonValue>(StringComparer.Ordinal);
            int buildings = 0;
            foreach (var element in elements.AsJsonArray)
            {
                string id = Field(element, "Guid").AsString, type = Field(element, "ElementType").AsString;
                if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(type) || nodes.ContainsKey(id)) throw new InvalidDataException("A blueprint building has a missing or repeated identifier/type.");
                nodes.Add(id, element); Point(Field(element, "Position"));
                if (!Field(element, "Inputs").IsJsonArray || !Field(element, "Outputs").IsJsonArray) throw new InvalidDataException("A building's connection ports are missing.");
                var stack = Field(element, "StackSize"); int count = stack.IsNull ? 1 : stack.AsInteger;
                if ((!stack.IsNull && !stack.IsInteger) || count < 1 || count > 5) throw new InvalidDataException("Blueprint stacks must contain between 1 and 5 buildings.");
                buildings = checked(buildings + count);
                if (!Field(element, "Upgrades").IsNull && !Field(element, "Upgrades").IsJsonArray) throw new InvalidDataException("Invalid building upgrade list.");
            }
            var usedInputs = new HashSet<string>(StringComparer.Ordinal); var usedOutputs = new HashSet<string>(StringComparer.Ordinal);
            foreach (var pipe in pipes.AsJsonArray)
            {
                if (!usedOutputs.Add(Endpoint(Field(pipe, "ConnectedOutput"), nodes, "Outputs"))
                    || !usedInputs.Add(Endpoint(Field(pipe, "ConnectedInput"), nodes, "Inputs")))
                    throw new InvalidDataException("Multiple connections use the same building port.");
                if (!Field(pipe, "Nodes").IsJsonArray) throw new InvalidDataException("A connection is missing its bend list.");
                foreach (var point in pipe["Nodes"].AsJsonArray) Point(point);
            }
            foreach (var deco in decos.AsJsonArray)
            {
                Point(Field(deco, "Position")); Point(Field(deco, "Size"));
                if (deco["Size"][0].AsInteger <= 0 || deco["Size"][1].AsInteger <= 0) throw new InvalidDataException("Invalid decoration size.");
            }
            string name = Field(value, "name").AsString;
            name = BlueprintSuggestions.Create(new[] { name }, null).DefaultName("Imported blueprint");
            return new PortableBlueprint { Document = value.AsJsonObject, Name = name, Icon = Field(value, "icon").AsString ?? "",
                Buildings = buildings, Pipes = pipes.AsJsonArray.Count, Decorations = decos.AsJsonArray.Count };
        }
        static void Point(JsonValue point)
        {
            if (!point.IsJsonArray || point.AsJsonArray.Count != 2 || !point[0].IsInteger || !point[1].IsInteger
                || Math.Abs(point[0].AsNumber) > 10000000 || Math.Abs(point[1].AsNumber) > 10000000)
                throw new InvalidDataException("Invalid blueprint coordinates.");
        }
        static string Endpoint(JsonValue endpoint, Dictionary<string, JsonValue> nodes, string ports)
        {
            string id = Field(endpoint, "Guid").AsString;
            var index = Field(endpoint, "Index");
            if (id == null || !nodes.TryGetValue(id, out var node) || !index.IsInteger || index.AsInteger < 0 || index.AsInteger >= node[ports].AsJsonArray.Count)
                throw new InvalidDataException("A connection points to a missing building or port.");
            return id + "/" + index.AsInteger;
        }
        public static IEnumerable<(string key, string value)> References(JsonValue data, string key = "")
        {
            if (data.IsString)
            {
                if (key != "Guid" && key != "ElementType" && key != "IconName" && key != "Label" && !string.IsNullOrEmpty(data.AsString)) yield return (key, data.AsString);
            }
            else if (data.IsJsonObject)
                foreach (var pair in data.AsJsonObject) foreach (var reference in References(pair.Value, pair.Key)) yield return reference;
            else if (data.IsJsonArray)
                foreach (var child in data.AsJsonArray) foreach (var reference in References(child, key)) yield return reference;
        }
    }
    public sealed class BlueprintResearchIssue
    {
        public string Id, Name, Description;
        public readonly HashSet<string> Affected = new HashSet<string>(StringComparer.Ordinal);
    }
    public sealed class BlueprintResearchReport
    {
        public readonly List<BlueprintResearchIssue> Issues = new List<BlueprintResearchIssue>();
        public bool CanPlace => Issues.Count == 0;
        public void Add(string id, string name, string description, string affected)
        {
            var issue = Issues.FirstOrDefault(i => i.Id == id);
            if (issue == null) { issue = new BlueprintResearchIssue { Id = id, Name = name, Description = description }; Issues.Add(issue); }
            issue.Affected.Add(affected);
        }
    }
}
