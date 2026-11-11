using System.Collections.Generic;
using System.Linq;
using Assets.Source.Flow;
using Assets.Source.Util;
using UnityEngine;

namespace SharableBlueprints
{
    public static partial class SelectionContents
    {
        public static SelectionPreview Preview(FlowDiagram flow, SelectionBox bounds)
        {
            var preview = SelectionPreview.Count(Describe(flow, bounds), bounds);
            var decorations = Decorations(flow, bounds).ToList();
            preview.Containers = decorations.Count(d => d.IsContainer);
            preview.Decorations = decorations.Count - preview.Containers;
            return preview;
        }
    }
}
