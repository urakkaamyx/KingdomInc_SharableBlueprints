using System.Collections.Generic;
using System.Linq;
using Assets.Source.Flow;
using Assets.Source.Util;
using UnityEngine;

namespace SharableBlueprints
{
    public static partial class SelectionContents
    {
        public static IEnumerable<FlowDeco> Decorations(FlowDiagram flow, SelectionBox bounds)
        {
            foreach (var deco in flow.Decorations)
                if (deco.IsContainer ? bounds.EnclosesContainer(deco.Position.x, deco.Position.y, deco.Size.x, deco.Size.y)
                    : bounds.Encloses(deco.Position.x, deco.Position.y, deco.Size.x, deco.Size.y)) yield return deco;
        }
    }
}
