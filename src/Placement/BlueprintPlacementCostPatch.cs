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
        [HarmonyPatch(typeof(WorldFlowGhost), nameof(WorldFlowGhost.GetCopyCost), new Type[] { })]
    static partial class BlueprintPlacementCostPatch
    {
        static bool Prefix(WorldFlowGhost __instance, ref CurrencyCollection __result)
        {
            var placement = __instance.GetComponent<BlueprintPlacement>();
            if (!placement) return true;
            __result = placement.Cost(); return false;
        }
    }
}
