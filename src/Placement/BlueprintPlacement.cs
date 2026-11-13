using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using KingdomFoundation.Api;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SharableBlueprints
{
    public sealed class BlueprintPlacement : MonoBehaviour
    {
        public static void Begin(SharableBlueprintsPlugin owner, PortableBlueprint definition, string entryId)
        {
            var source = FoundationApi.Blueprints.DeserializeWorkspace(definition.Document["layout"]);
            if (source.Buildings.Count == 0 || source.Pipes.Count != definition.Pipes || source.Pipes.Contains(null))
                throw new InvalidDataException("This blueprint has connections that the installed game cannot load.");
            var manager = FoundationApi.WorkspaceAccess.Root;
            FoundationApi.UiAccess.CancelDragDrop();
            var root = FoundationApi.WorkspaceAccess.ShowPurchaseBlueprint(source.Buildings[0], false);
            try
            {
                var tag = root.gameObject.AddComponent<BlueprintPlacement>();
                tag.owner = owner;
                tag.definition = definition;
                tag.source = source;
                tag.player = FoundationApi.Kingdom;
                tag.destination = FoundationApi.Kingdom.ActiveWorkspace;
                tag.entryId = entryId;
                var rootRect = (RectTransform)root.transform;
                Vector2Int origin = source.Buildings[0].Position - new Vector2Int(0, Mathf.RoundToInt(rootRect.sizeDelta.y));
                for (int i = 1; i < source.Buildings.Count; i++)
                {
                    var child = FoundationApi.Blueprints.CreateChild(root);
                    FoundationApi.Blueprints.SetBuilding(child, source.Buildings[i], false);
                    FoundationApi.Blueprints.SetChildOffset(child, origin);
                }

                foreach (var deco in source.Decorations)
                {
                    var child = FoundationApi.Blueprints.CreateChild(root);
                    FoundationApi.Blueprints.SetDecoration(child, deco, false, false);
                    FoundationApi.Blueprints.SetChildOffset(child, origin);
                }

                FoundationApi.Blueprints.UpdateDragPosition();
                FoundationApi.Blueprints.UpdateCostDisplay(tag.Cost());
            }
            catch
            {
                FoundationApi.WorkspaceAccess.StopBlueprintMode();
                throw;
            }
        }

        public bool CheckBeforeCommit(Component root)
        {
            try
            {
                if (player != FoundationApi.Kingdom || !FoundationApi.UiAccess.IsWorkspaceVisible || destination != FoundationApi.Kingdom.ActiveWorkspace)
                {
                    Stop("Blueprint placement cancelled because the active kingdom or workshop changed.");
                    return false;
                }

                var research = BlueprintResearch.Check(definition);
                if (!research.CanPlace)
                {
                    FoundationApi.WorkspaceAccess.StopBlueprintMode();
                    owner.ShowResearchBlocked(entryId, research);
                    return false;
                }

                if (!player.CanAfford(Cost()))
                {
                    FoundationApi.WorkspaceAccess.StopBlueprintMode();
                    FoundationApi.UiAccess.ShowInsufficientFunds();
                    return false;
                }

                var rootRect = (RectTransform)root.transform;
                foreach (var ghost in FoundationApi.Blueprints.Children(root))
                {
                    var element = FoundationApi.Blueprints.Building(ghost);
                    if (element == null)
                        continue;
                    var rect = (RectTransform)ghost.transform;
                    Vector2 position = rect.anchoredPosition;
                    if (ghost != root)
                        position += rootRect.anchoredPosition - new Vector2(0, rootRect.sizeDelta.y);
                    if (FoundationApi.Blueprints.Overlaps(new Vector2Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y)), new Vector2Int(Mathf.RoundToInt(rect.sizeDelta.x), Mathf.RoundToInt(rect.sizeDelta.y))))
                    {
                        Stop("Not enough space for the whole blueprint. No buildings were placed.");
                        return false;
                    }

                    FoundationApi.Blueprints.SetCollision(ghost, false);
                }

                return true;
            }
            catch (Exception error)
            {
                owner.LogLibraryError(error);
                Stop("Blueprint could not be placed. No placement was started.");
                return false;
            }
        }

        public GameMoney Cost()
        {
            var total = new GameMoney();
            var counts = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (var element in source.Buildings)
            {
                counts.TryGetValue(element.TypeId, out int count);
                total.Add(FoundationApi.WorkspaceAccess.CopyCost(element, count));
                counts[element.TypeId] = count + Math.Max(1, element.StackSize);
            }

            foreach (var deco in source.Decorations)
                total.Add(FoundationApi.WorkspaceAccess.CopyCost(deco));
            return total;
        }

        SharableBlueprintsPlugin owner;
        PortableBlueprint definition;
        GameWorkspace source, destination;
        GameKingdom player;
        string entryId;
        public static void CancelOwned(SharableBlueprintsPlugin owner)
        {
            var manager = FoundationApi.WorkspaceAccess.Root;
            if (!manager)
                return;
            var ghost = FoundationApi.Blueprints.CurrentGhost;
            var placement = ghost ? ghost.GetComponent<BlueprintPlacement>() : null;
            if (placement && placement.owner == owner)
                FoundationApi.WorkspaceAccess.StopBlueprintMode();
        }

        void Stop(string message)
        {
            FoundationApi.WorkspaceAccess.StopBlueprintMode();
            owner.ShowMessage(message);
        }
    }
}
