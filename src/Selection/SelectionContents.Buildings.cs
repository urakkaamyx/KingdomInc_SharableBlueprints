using System.Collections.Generic;
using System.Linq;
using Assets.Source.Flow;
using Assets.Source.Util;
using UnityEngine;

namespace SharableBlueprints
{
    public static partial class SelectionContents
    {
        public static IEnumerable<FlowElement> Buildings(FlowDiagram flow, SelectionBox bounds)
        {
            foreach (var element in flow.Elements)
            {
                var size = Size(element);
                if (bounds.Encloses(element.Position.x, element.Position.y, size.x, size.y)) yield return element;
            }
        }
    }
}
