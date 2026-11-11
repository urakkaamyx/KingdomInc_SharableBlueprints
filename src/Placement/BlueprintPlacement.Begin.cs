using System;
using System.Collections.Generic;
using System.IO;
using Assets.Behaviour.Flow;
using Assets.Behaviour.UI;
using Assets.Source.Flow;
using Assets.Source.Player;
using HarmonyLib;
using KingdomFoundation.Api;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SharableBlueprints
{
    public sealed partial class BlueprintPlacement : MonoBehaviour
    {
        public static void Begin(SharableBlueprintsPlugin owner, PortableBlueprint definition, string entryId)
        {
            var source = FlowDiagram.FromJson(definition.Document["layout"]);
            if (source.Elements.Count == 0 || source.Pipes.Count != definition.Pipes || source.Pipes.Contains(null))
                throw new InvalidDataException("This blueprint has connections that the installed game cannot load.");
            var manager = WorldFlowManager.Instance;
            DragDropManager.Instance.CancelDragDrop();
            var root = manager.ShowPurchaseBlueprint(source.Elements[0], false);
            try
            {
                var tag = root.gameObject.AddComponent<BlueprintPlacement>();
                tag.owner = owner; tag.definition = definition; tag.source = source; tag.player = FoundationApi.World.Player;
                tag.destination = manager.CurrentFlow; tag.entryId = entryId;
                var rootRect = (RectTransform)root.transform;
                Vector2Int origin = source.Elements[0].Position - new Vector2Int(0, Mathf.RoundToInt(rootRect.sizeDelta.y));
                for (int i = 1; i < source.Elements.Count; i++)
                {
                    var child = Object.Instantiate(manager.BlueprintPrefab, root.transform);
                    child.SetElement(source.Elements[i], false); child.SetChildOffset(origin);
                }
                foreach (var deco in source.Decorations)
                {
                    var child = Object.Instantiate(manager.BlueprintPrefab, root.transform);
                    child.SetDeco(deco, false, false); child.SetChildOffset(origin);
                }
                DragDropManager.Instance.UpdateDragPosition();
                manager.UpdateCostDisplay(tag.Cost());
            }
            catch { manager.StopBlueprintMode(); throw; }
        }
    }
}
