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
    }
}
