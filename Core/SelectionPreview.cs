using System;
using System.Collections.Generic;
using System.Linq;

namespace SharableBlueprints
{
    public static class SelectionEdgePan
    {
        // Screen coordinates increase upward, like the native camera's Move method.
        public static double Axis(double position, double extent, double margin)
        {
            if (double.IsNaN(position) || double.IsNaN(extent) || double.IsNaN(margin)
                || double.IsInfinity(position) || double.IsInfinity(extent) || double.IsInfinity(margin)
                || extent <= 0 || margin <= 0 || position < 0 || position > extent) return 0;
            margin = Math.Min(margin, extent / 2);
            if (position < margin) return -(margin - position) / margin;
            if (position > extent - margin) return (position - (extent - margin)) / margin;
            return 0;
        }
    }
    public sealed class SelectionBuilding
    {
        public string Type, Name;
        public int X, Y, Width, Height, StackSize = 1;
    }
    public sealed class SelectionBuildingCount
    {
        public string Type, Name;
        public int Buildings, Nodes;
    }
    public sealed class SelectionPreview
    {
        public List<SelectionBuildingCount> Groups;
        public int Buildings, Nodes;
        public int Containers, Decorations;
        public static SelectionPreview Count(IEnumerable<SelectionBuilding> buildings, SelectionBox bounds)
        {
            var groups = new Dictionary<string, SelectionBuildingCount>(StringComparer.Ordinal);
            foreach (var building in buildings)
            {
                if (!bounds.Encloses(building.X, building.Y, building.Width, building.Height)) continue;
                string type = building.Type ?? "Unknown";
                if (!groups.TryGetValue(type, out var group))
                    groups.Add(type, group = new SelectionBuildingCount { Type = type, Name = building.Name ?? type });
                group.Nodes++; group.Buildings += Math.Max(1, building.StackSize);
            }
            var result = new SelectionPreview { Groups = groups.Values.OrderBy(g => g.Name, StringComparer.OrdinalIgnoreCase).ThenBy(g => g.Type, StringComparer.Ordinal).ToList() };
            result.Buildings = result.Groups.Sum(g => g.Buildings); result.Nodes = result.Groups.Sum(g => g.Nodes);
            return result;
        }
    }
}
