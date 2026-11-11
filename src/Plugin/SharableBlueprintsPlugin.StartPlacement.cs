using System;
using System.IO;
using System.Reflection;
using Assets.Behaviour.Flow;
using Assets.Behaviour.UI;
using Assets.Behaviour.UI.BuyMenu;
using Assets.Behaviour.Util;
using Assets.Source.Player;
using Behaviour.UI.Tooltip;
using BepInEx;
using UnityEngine;
using UnityEngine.UI;
using GameInput = Assets.Behaviour.Input.PlayerInput;
using HarmonyLib;
using KingdomFoundation.Api;

namespace SharableBlueprints
{
    public sealed partial class SharableBlueprintsPlugin : BaseUnityPlugin
    {
        internal void StartPlacement(PortableBlueprint blueprint, string entryId)
        {
            try
            {
                if (!placementHooksReady) throw new InvalidOperationException("The blueprint placement guards are unavailable. Check the BepInEx log.");
                var report = BlueprintResearch.Check(blueprint);
                if (!report.CanPlace) { library.ShowResearch(report); return; }
                library.ClosePopup(); BlueprintPlacement.Begin(this, blueprint, entryId);
            }
            catch (Exception error) { LogLibraryError(error); library.Open(entryId, "Could not prepare this blueprint: " + error.Message); }
        }
    }
}
