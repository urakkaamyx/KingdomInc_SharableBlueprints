using System.Collections.Generic;
using System.Linq;
using Assets.Source.Flow;
using Assets.Source.Util;
using UnityEngine;

namespace SharableBlueprints
{
    public static class SelectionContents
    {
        static Vector2Int Size(FlowElement element)
        {
            Vector2 size = element.World ? element.World.Size : (Vector2)element.Size;
            return new Vector2Int(Mathf.CeilToInt(size.x), Mathf.CeilToInt(size.y));
        }
        public static IEnumerable<FlowElement> Buildings(FlowDiagram flow, SelectionBox bounds)
        {
            foreach (var element in flow.Elements)
            {
                var size = Size(element);
                if (bounds.Encloses(element.Position.x, element.Position.y, size.x, size.y)) yield return element;
            }
        }
        public static SelectionPreview Preview(FlowDiagram flow, SelectionBox bounds)
        {
            var preview = SelectionPreview.Count(Describe(flow, bounds), bounds);
            var decorations = Decorations(flow, bounds).ToList();
            preview.Containers = decorations.Count(d => d.IsContainer);
            preview.Decorations = decorations.Count - preview.Containers;
            return preview;
        }
        public static IEnumerable<FlowDeco> Decorations(FlowDiagram flow, SelectionBox bounds)
        {
            foreach (var deco in flow.Decorations)
                if (deco.IsContainer ? bounds.EnclosesContainer(deco.Position.x, deco.Position.y, deco.Size.x, deco.Size.y)
                    : bounds.Encloses(deco.Position.x, deco.Position.y, deco.Size.x, deco.Size.y)) yield return deco;
        }
        static IEnumerable<SelectionBuilding> Describe(FlowDiagram flow, SelectionBox bounds)
        {
            foreach (var element in Buildings(flow, bounds))
            {
                var size = Size(element);
                yield return new SelectionBuilding { Type = element.ElementType, Name = Translation.Translate(element.Label),
                    X = element.Position.x, Y = element.Position.y, Width = size.x, Height = size.y, StackSize = element.StackSize };
            }
        }
    }
}
