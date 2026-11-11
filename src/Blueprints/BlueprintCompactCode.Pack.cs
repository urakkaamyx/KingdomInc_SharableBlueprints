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
    }
}
