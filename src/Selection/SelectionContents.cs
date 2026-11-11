using System.Collections.Generic;
using System.Linq;
using Assets.Source.Flow;
using Assets.Source.Util;
using UnityEngine;

namespace SharableBlueprints
{
    public static partial class SelectionContents
    {
        static Vector2Int Size(FlowElement element)
        {
            Vector2 size = element.World ? element.World.Size : (Vector2)element.Size;
            return new Vector2Int(Mathf.CeilToInt(size.x), Mathf.CeilToInt(size.y));
        }
    }
}
