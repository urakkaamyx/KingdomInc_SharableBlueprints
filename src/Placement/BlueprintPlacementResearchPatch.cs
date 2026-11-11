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
        [HarmonyPatch(typeof(WorldFlowGhost), nameof(WorldFlowGhost.OnDragDropEnd))]
    static partial class BlueprintPlacementResearchPatch
    {
        static bool Prefix(WorldFlowGhost __instance)
        {
            var placement = __instance.GetComponent<BlueprintPlacement>();
            return !placement || placement.CheckBeforeCommit(__instance);
        }
    }
}
