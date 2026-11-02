using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LightJson;

namespace SharableBlueprints
{
    // KIB2 binary payload, then gzip. IDs are local node indices; identical
    // building settings are stored once. Geometry and every setting are retained.
    public static class BlueprintCompactCode
    {
        const int Max = 8 * 1024 * 1024;
        static readonly Encoding Utf8 = new UTF8Encoding(false, true);
        static int Gcd(int a, int b) { a = Math.Abs(a); b = Math.Abs(b); while (b != 0) { int r = a % b; a = b; b = r; } return a; }
        static int Unit(BinaryReader reader) { uint n = Var(reader); if (n == 0 || n > 20000000) throw new InvalidDataException("Invalid coordinate unit."); return (int)n; }
        static JsonObject Except(JsonObject value, params string[] keys)
        {
            var result = new JsonObject();
            foreach (var pair in ((IEnumerable<KeyValuePair<string, JsonValue>>)value).OrderBy(p => p.Key, StringComparer.Ordinal)) if (!keys.Contains(pair.Key)) result[pair.Key] = pair.Value;
            return result;
        }
        static string Canonical(JsonValue value)
        {
            if (value.IsJsonObject)
            {
                var sorted = new JsonObject();
                foreach (var pair in ((IEnumerable<KeyValuePair<string, JsonValue>>)value.AsJsonObject).OrderBy(p => p.Key, StringComparer.Ordinal)) sorted[pair.Key] = JsonValue.Parse(Canonical(pair.Value));
                return sorted.ToString(false);
            }
            if (value.IsJsonArray) return new JsonArray(value.AsJsonArray.Select(v => JsonValue.Parse(Canonical(v))).ToArray()).ToString(false);
            return value.ToString(false);
        }
        static void Var(BinaryWriter writer, uint number)
        {
            while (number >= 128) { writer.Write((byte)(number | 128)); number >>= 7; }
            writer.Write((byte)number);
        }
        static uint Var(BinaryReader reader)
        {
            uint result = 0;
            for (int shift = 0; shift < 35; shift += 7)
            {
                byte part = reader.ReadByte();
                if (shift == 28 && part > 15) throw new InvalidDataException("Invalid compact blueprint integer.");
                result |= (uint)(part & 127) << shift;
                if (part < 128) return result;
            }
            throw new InvalidDataException("Invalid compact blueprint integer.");
        }
        static int Count(BinaryReader reader) { uint n = Var(reader); if (n > 100000) throw new InvalidDataException("Compact blueprint list is too large."); return (int)n; }
        static void Signed(BinaryWriter writer, int n) => Var(writer, unchecked((uint)((n << 1) ^ (n >> 31))));
        static int Signed(BinaryReader reader) { uint n = Var(reader); return unchecked((int)(n >> 1) ^ -((int)n & 1)); }
        static void Text(BinaryWriter writer, string text)
        {
            byte[] bytes = Utf8.GetBytes(text); Var(writer, (uint)bytes.Length); writer.Write(bytes);
        }
        static string Text(BinaryReader reader)
        {
            uint length = Var(reader);
            if (length > Max || length > reader.BaseStream.Length - reader.BaseStream.Position) throw new InvalidDataException("Invalid compact blueprint text length.");
            return Utf8.GetString(reader.ReadBytes((int)length));
        }
        public static byte[] Pack(JsonObject document)
        {
            var layout = document["layout"].AsJsonObject; var elements = layout["Elements"].AsJsonArray;
            var templates = new List<string>(); var indices = new List<int>(); var ids = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (var element in elements)
            {
                string template = Canonical(Except(element.AsJsonObject, "Guid", "Position"));
                int index = templates.IndexOf(template);
                if (index < 0) { index = templates.Count; templates.Add(template); }
                indices.Add(index); ids.Add(element["Guid"].AsString, ids.Count);
            }
            using (var output = new MemoryStream())
            using (var writer = new BinaryWriter(output, Utf8, true))
            {
                writer.Write(new byte[] { 75, 73, 66, 50 });
                // Suggestions can be rebuilt from the captured labels/stickers and
                // building types. Export timestamps/version do not affect placement.
                Text(writer, Canonical(Except(document, "layout", "nameOptions", "iconOptions", "createdAtUtc", "modVersion")));
                Text(writer, Canonical(Except(layout, "Elements", "Pipes", "Decorations")));
                Var(writer, (uint)templates.Count); foreach (string template in templates) Text(writer, template);
                int x = elements[0]["Position"][0].AsInteger, y = elements[0]["Position"][1].AsInteger, unit = 0;
                foreach (var node in elements) { unit = Gcd(unit, node["Position"][0].AsInteger - x); unit = Gcd(unit, node["Position"][1].AsInteger - y); }
                unit = Math.Max(1, unit); Var(writer, (uint)unit); Signed(writer, x); Signed(writer, y);
                Var(writer, (uint)elements.Count);
                for (int i = 0; i < elements.Count; i++)
                {
                    int nx = elements[i]["Position"][0].AsInteger, ny = elements[i]["Position"][1].AsInteger;
                    Var(writer, (uint)indices[i]); Signed(writer, checked(nx - x) / unit); Signed(writer, checked(ny - y) / unit); x = nx; y = ny;
                }
                var pipes = layout["Pipes"].AsJsonArray; int bendUnit = 0;
                foreach (var pipe in pipes)
                {
                    int source = ids[pipe["ConnectedOutput"]["Guid"].AsString];
                    x = elements[source]["Position"][0].AsInteger; y = elements[source]["Position"][1].AsInteger;
                    foreach (var bend in pipe["Nodes"].AsJsonArray)
                    {
                        int nx = bend[0].AsInteger, ny = bend[1].AsInteger;
                        bendUnit = Gcd(bendUnit, nx - x); bendUnit = Gcd(bendUnit, ny - y); x = nx; y = ny;
                    }
                }
                bendUnit = Math.Max(1, bendUnit); Var(writer, (uint)bendUnit); Var(writer, (uint)pipes.Count);
                foreach (var pipe in pipes)
                {
                    int source = ids[pipe["ConnectedOutput"]["Guid"].AsString], target = ids[pipe["ConnectedInput"]["Guid"].AsString];
                    Var(writer, (uint)source); Var(writer, (uint)pipe["ConnectedOutput"]["Index"].AsInteger);
                    Var(writer, (uint)target); Var(writer, (uint)pipe["ConnectedInput"]["Index"].AsInteger);
                    var bends = pipe["Nodes"].AsJsonArray; Var(writer, (uint)bends.Count);
                    x = elements[source]["Position"][0].AsInteger; y = elements[source]["Position"][1].AsInteger;
                    foreach (var bend in bends)
                    {
                        int nx = bend[0].AsInteger, ny = bend[1].AsInteger;
                        Signed(writer, checked(nx - x) / bendUnit); Signed(writer, checked(ny - y) / bendUnit); x = nx; y = ny;
                    }
                    Text(writer, Canonical(Except(pipe.AsJsonObject, "ConnectedOutput", "ConnectedInput", "Nodes")));
                }
                Text(writer, Canonical(layout["Decorations"]));
                if (output.Length > Max) throw new InvalidDataException("Compact blueprint exceeds 8 MB.");
                return output.ToArray();
            }
        }
        public static string Unpack(byte[] bytes)
        {
            try
            {
                using (var input = new MemoryStream(bytes))
                using (var reader = new BinaryReader(input, Utf8))
                {
                    if (!reader.ReadBytes(4).SequenceEqual(new byte[] { 75, 73, 66, 50 })) throw new InvalidDataException("Unknown compact blueprint version.");
                    var document = JsonValue.Parse(Text(reader)).AsJsonObject; var layout = JsonValue.Parse(Text(reader)).AsJsonObject;
                    int templateCount = Count(reader); var templates = new List<string>();
                    for (int i = 0; i < templateCount; i++) templates.Add(Text(reader));
                    int unit = Unit(reader), x = Signed(reader), y = Signed(reader);
                    int nodeCount = Count(reader); var nodes = new JsonArray(); long expanded = 0;
                    for (int i = 0; i < nodeCount; i++)
                    {
                        uint index = Var(reader); if (index >= templates.Count) throw new InvalidDataException("Invalid building template.");
                        expanded += Utf8.GetByteCount(templates[(int)index]) + 100;
                        if (expanded > Max) throw new InvalidDataException("Expanded blueprint exceeds 8 MB.");
                        var node = JsonValue.Parse(templates[(int)index]).AsJsonObject;
                        x = checked(x + Signed(reader) * unit); y = checked(y + Signed(reader) * unit);
                        node["Guid"] = "n" + i; node["Position"] = new JsonArray(x, y); nodes.Add(node);
                    }
                    int bendUnit = Unit(reader), pipeCount = Count(reader); var pipes = new JsonArray();
                    for (int i = 0; i < pipeCount; i++)
                    {
                        uint source = Var(reader), output = Var(reader), target = Var(reader), port = Var(reader);
                        if (source >= nodes.Count || target >= nodes.Count || output > int.MaxValue || port > int.MaxValue) throw new InvalidDataException("Invalid compact connection.");
                        int bendCount = Count(reader); var bends = new JsonArray();
                        x = nodes[(int)source]["Position"][0].AsInteger; y = nodes[(int)source]["Position"][1].AsInteger;
                        for (int j = 0; j < bendCount; j++)
                        {
                            expanded += 32; if (expanded > Max) throw new InvalidDataException("Expanded blueprint exceeds 8 MB.");
                            x = checked(x + Signed(reader) * bendUnit); y = checked(y + Signed(reader) * bendUnit); bends.Add(new JsonArray(x, y));
                        }
                        expanded += 256; if (expanded > Max) throw new InvalidDataException("Expanded blueprint exceeds 8 MB.");
                        var pipe = JsonValue.Parse(Text(reader)).AsJsonObject;
                        pipe["ConnectedOutput"] = new JsonObject {{"Guid", "n" + source}, {"Index", (int)output}};
                        pipe["ConnectedInput"] = new JsonObject {{"Guid", "n" + target}, {"Index", (int)port}};
                        pipe["Nodes"] = bends; pipes.Add(pipe);
                    }
                    layout["Elements"] = nodes; layout["Pipes"] = pipes; layout["Decorations"] = JsonValue.Parse(Text(reader));
                    if (input.Position != input.Length) throw new InvalidDataException("Unexpected data after compact blueprint.");
                    document["layout"] = layout; string json = document.ToString(false);
                    if (Utf8.GetByteCount(json) > Max) throw new InvalidDataException("Expanded blueprint exceeds 8 MB.");
                    return json;
                }
            }
            catch (InvalidDataException) { throw; }
            catch (Exception error) when (error is EndOfStreamException || error is OverflowException || error is InvalidOperationException || error is ArgumentException)
            { throw new InvalidDataException("The compact blueprint is incomplete or damaged.", error); }
        }
    }
}
