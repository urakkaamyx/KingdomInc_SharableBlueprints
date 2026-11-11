using System.Collections.Generic;
using System.Linq;
using Assets.Source.Flow;
using Assets.Source.Util;
using UnityEngine;

namespace SharableBlueprints
{
    public static partial class SelectionContents
    {
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
