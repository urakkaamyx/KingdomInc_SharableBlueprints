using System;
using System.Collections.Generic;
using System.IO;
using Assets.Behaviour.Flow;
using Assets.Behaviour.UI;
using Assets.Source.Flow;
using Assets.Source.Player;
using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SharableBlueprints
{
    public sealed class BlueprintPlacement : MonoBehaviour
    {
        SharableBlueprintsPlugin owner;
        PortableBlueprint definition;
        FlowDiagram source, destination;
        GamePlayer player;
        string entryId;
        public static void CancelOwned(SharableBlueprintsPlugin owner)
        {
            var manager = WorldFlowManager.Instance;
            if (!manager) return;
            var ghost = AccessTools.Field(typeof(WorldFlowManager), "_currentGhost").GetValue(manager) as WorldFlowGhost;
            var placement = ghost ? ghost.GetComponent<BlueprintPlacement>() : null;
            if (placement && placement.owner == owner) manager.StopBlueprintMode();
        }
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
                tag.owner = owner; tag.definition = definition; tag.source = source; tag.player = GamePlayer.Current;
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
        public CurrencyCollection Cost()
        {
            var total = new CurrencyCollection();
            var counts = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (var element in source.Elements)
            {
                counts.TryGetValue(element.ElementType, out int count);
                total.Add(element.GetCopyCost(count)); counts[element.ElementType] = count + Math.Max(1, element.StackSize);
            }
            foreach (var deco in source.Decorations) total.Add(deco.GetCopyCost(false));
            return total;
        }
        public bool CheckBeforeCommit(WorldFlowGhost root)
        {
            try
            {
                if (player != GamePlayer.Current || !WorldFlowManager.Visible || destination != WorldFlowManager.Instance.CurrentFlow)
                { Stop("Blueprint placement cancelled because the active kingdom or workshop changed."); return false; }
                var research = BlueprintResearch.Check(definition);
                if (!research.CanPlace)
                {
                    WorldFlowManager.Instance.StopBlueprintMode(); owner.ShowResearchBlocked(entryId, research); return false;
                }
                if (!player.CanAfford(Cost()))
                { WorldFlowManager.Instance.StopBlueprintMode(); GameUI.Instance.InsufficientFundsWarning(); return false; }
                var rootRect = (RectTransform)root.transform;
                foreach (var ghost in root.GetComponentsInChildren<WorldFlowGhost>())
                {
                    var element = AccessTools.Field(typeof(WorldFlowGhost), "_element").GetValue(ghost) as FlowElement;
                    if (element == null) continue;
                    var rect = (RectTransform)ghost.transform;
                    Vector2 position = rect.anchoredPosition;
                    if (ghost != root) position += rootRect.anchoredPosition - new Vector2(0, rootRect.sizeDelta.y);
                    if (FlowColliderManager.Instance.Overlaps(new Vector2Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y)),
                        new Vector2Int(Mathf.RoundToInt(rect.sizeDelta.x), Mathf.RoundToInt(rect.sizeDelta.y))))
                    { Stop("Not enough space for the whole blueprint. No buildings were placed."); return false; }
                    AccessTools.Field(typeof(WorldFlowGhost), "_collides").SetValue(ghost, false);
                }
                return true;
            }
            catch (Exception error) { owner.LogLibraryError(error); Stop("Blueprint could not be placed. No placement was started."); return false; }
        }
        void Stop(string message) { WorldFlowManager.Instance.StopBlueprintMode(); owner.ShowMessage(message); }
    }
    [HarmonyPatch(typeof(WorldFlowGhost), nameof(WorldFlowGhost.OnDragDropEnd))]
    static class BlueprintPlacementResearchPatch
    {
        static bool Prefix(WorldFlowGhost __instance)
        {
            var placement = __instance.GetComponent<BlueprintPlacement>();
            return !placement || placement.CheckBeforeCommit(__instance);
        }
    }
    [HarmonyPatch(typeof(WorldFlowGhost), nameof(WorldFlowGhost.GetCopyCost), new Type[] { })]
    static class BlueprintPlacementCostPatch
    {
        static bool Prefix(WorldFlowGhost __instance, ref CurrencyCollection __result)
        {
            var placement = __instance.GetComponent<BlueprintPlacement>();
            if (!placement) return true;
            __result = placement.Cost(); return false;
        }
    }
}
