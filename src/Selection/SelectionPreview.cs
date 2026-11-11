using System;
using System.Collections.Generic;
using System.Linq;

namespace SharableBlueprints
{
    public sealed partial class SelectionPreview
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
