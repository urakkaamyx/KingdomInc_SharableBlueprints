using System;
using System.Collections.Generic;
using System.IO;
using KingdomFoundation.Api;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SharableBlueprints
{
    static class BlueprintPlacementCostPatch
    {
        internal static bool Prefix(Component __instance, ref GameMoney __result)
        {
            var placement = __instance.GetComponent<BlueprintPlacement>();
            if (!placement)
                return true;
            __result = placement.Cost();
            return false;
        }
    }
}
