using System;
using System.Collections.Generic;
using System.IO;
using KingdomFoundation.Api;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SharableBlueprints
{
    static class BlueprintPlacementResearchPatch
    {
        internal static bool Prefix(Component __instance)
        {
            var placement = __instance.GetComponent<BlueprintPlacement>();
            return !placement || placement.CheckBeforeCommit(__instance);
        }
    }
}
