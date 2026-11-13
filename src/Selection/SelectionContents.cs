using KingdomFoundation.Api;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SharableBlueprints
{
    public static class SelectionContents
    {
        public static IEnumerable<WorkspaceObject> Buildings(GameWorkspace flow, SelectionBox bounds)
        {
            foreach (var element in flow.Buildings)
            {
                var size = Size(element);
                if (bounds.Encloses(element.Position.x, element.Position.y, size.x, size.y))
                    yield return element;
            }
        }

        static Vector2Int Size(WorkspaceObject element)
        {
            Vector2 size = element.World ? FoundationApi.WorkspaceVisuals.Size(element.World) : (Vector2)element.Size;
            return new Vector2Int(Mathf.CeilToInt(size.x), Mathf.CeilToInt(size.y));
        }

        public static IEnumerable<WorkspaceObject> Decorations(GameWorkspace flow, SelectionBox bounds)
        {
            foreach (var deco in flow.Decorations)
                if (deco.IsContainer ? bounds.EnclosesContainer(deco.Position.x, deco.Position.y, Mathf.RoundToInt(deco.Size.x), Mathf.RoundToInt(deco.Size.y)) : bounds.Encloses(deco.Position.x, deco.Position.y, Mathf.RoundToInt(deco.Size.x), Mathf.RoundToInt(deco.Size.y)))
                    yield return deco;
        }

        static IEnumerable<SelectionBuilding> Describe(GameWorkspace flow, SelectionBox bounds)
        {
            foreach (var element in Buildings(flow, bounds))
            {
                var size = Size(element);
                yield return new SelectionBuilding
                {
                    Type = element.TypeId,
                    Name = FoundationApi.GameInfo.Translate(element.Label),
                    X = element.Position.x,
                    Y = element.Position.y,
                    Width = size.x,
                    Height = size.y,
                    StackSize = element.StackSize
                };
            }
        }

        public static SelectionPreview Preview(GameWorkspace flow, SelectionBox bounds)
        {
            var preview = SelectionPreview.Count(Describe(flow, bounds), bounds);
            var decorations = Decorations(flow, bounds).ToList();
            preview.Containers = decorations.Count(d => d.IsContainer);
            preview.Decorations = decorations.Count - preview.Containers;
            return preview;
        }
    }
}
