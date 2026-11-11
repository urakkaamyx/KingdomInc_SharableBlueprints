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
        internal void Export(SelectionBox bounds)
        {
            try
            {
                var blueprint = BlueprintCapture.Capture(WorldFlowManager.Instance.CurrentFlow, bounds);
                var entry = library.Save(blueprint, captureBookId);
                Logger.LogInfo("Saved " + entry.FileName + " (" + blueprint.Buildings + " buildings, " + blueprint.Pipes + " internal pipes). Save data was not modified.");
                library.Open(entry.Id, "Blueprint stored. Use a label or type a name, then choose its icon.");
            }
            catch (Exception error)
            {
                Logger.LogWarning("Blueprint export: " + error);
                Notify(error is InvalidOperationException ? error.Message : "Blueprint could not be saved. Check the BepInEx log.");
            }
        }
    }
}
