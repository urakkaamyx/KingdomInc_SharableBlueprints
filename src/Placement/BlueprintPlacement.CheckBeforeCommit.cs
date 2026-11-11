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
        public bool CheckBeforeCommit(WorldFlowGhost root)
        {
            try
            {
                if (player != FoundationApi.World.Player || !WorldFlowManager.Visible || destination != WorldFlowManager.Instance.CurrentFlow)
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
    }
}
