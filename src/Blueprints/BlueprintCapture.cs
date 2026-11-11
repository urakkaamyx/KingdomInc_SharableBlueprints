using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Source.Flow;
using LightJson;
using UnityEngine;

namespace SharableBlueprints
{
    public static partial class BlueprintCapture
    {
        public static CapturedBlueprint Capture(FlowDiagram flow, SelectionBox bounds)
        {
            if (flow == null || !bounds.HasArea) throw new InvalidOperationException("Drag around the buildings to include them.");
            var ids = new Dictionary<FlowElement, string>();
            var elements = new JsonArray(); var pipes = new JsonArray(); var decos = new JsonArray();
            var decorations = SelectionContents.Decorations(flow, bounds).ToList();
            var outputBounds = bounds;
            foreach (var deco in decorations)
                outputBounds = outputBounds.IncludeFrame(deco.Position.x, deco.Position.y, deco.Size.x, deco.Size.y);
            var origin = new Vector2Int(outputBounds.Left, outputBounds.Top);
            int buildingCount = 0;
            foreach (var element in SelectionContents.Buildings(flow, bounds))
            {
                buildingCount += Math.Max(1, element.StackSize);
                // Native copy semantics preserve settings/upgrades, not inventory or rewards.
                var copy = element.CreateCopy();
                copy.Position = element.Position - origin;
                ids.Add(element, copy.Guid);
                elements.Add(copy.ToJson());
            }
            if (elements.Count == 0) throw new InvalidOperationException("No complete buildings selected. Drag around their full frames.");
            foreach (var pipe in flow.Pipes)
            {
                if (pipe.ConnectedOutput == null || pipe.ConnectedInput == null) continue;
                if (!ids.TryGetValue(pipe.ConnectedOutput.Parent, out var source) || !ids.TryGetValue(pipe.ConnectedInput.Parent, out var target)) continue;
                var nodes = new JsonArray();
                foreach (var node in pipe.Nodes) nodes.Add(new JsonArray { node.x - origin.x, node.y - origin.y });
                pipes.Add(new JsonObject {
                    {"ConnectedOutput", new JsonObject {{"Guid", source}, {"Index", pipe.ConnectedOutput.ParentIndex}}},
                    {"ConnectedInput", new JsonObject {{"Guid", target}, {"Index", pipe.ConnectedInput.ParentIndex}}}, {"Nodes", nodes}
                });
            }
            foreach (var deco in decorations)
            {
                var copy = deco.CreateCopy(); copy.Locked = deco.Locked;
                copy.Position = deco.Position - origin;
                decos.Add(copy.ToJson());
            }
            return new CapturedBlueprint { Buildings = buildingCount, Pipes = pipes.Count, Decorations = decos.Count,
                Suggestions = BlueprintIdentity.FromSelection(flow, bounds),
                Document = new JsonObject {
                    {"format", "kingdominc.sharable-blueprint"}, {"formatVersion", 1}, {"modVersion", SharableBlueprintsPlugin.Version},
                    {"name", "Untitled blueprint"}, {"createdAtUtc", DateTime.UtcNow.ToString("o")},
                    {"size", new JsonArray {outputBounds.Width, outputBounds.Height}},
                    {"layout", new JsonObject {{"Elements", elements}, {"Pipes", pipes}, {"Decorations", decos}}}
                }
            };
        }
    }
}
