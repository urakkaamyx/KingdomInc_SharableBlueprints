using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LightJson;

namespace SharableBlueprints
{
    public static partial class BlueprintCompactCode
    {
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
