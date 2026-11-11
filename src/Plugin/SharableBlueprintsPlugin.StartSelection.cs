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
        internal void StartSelection(string destination)
        {
            if (!selection || !WorldFlowManager.Visible) return;
            try
            {
                captureBookId = destination;
                DragDropManager.Instance.CancelDragDrop();
                GameUI.Instance.CloseAllExcept(null);
                selection.Begin();
                Notify("Drag an area. Hold at a screen edge to pan. Escape cancels.");
            }
            catch (Exception error) { selection.Cancel(); Logger.LogError(error); Notify("Could not start blueprint selection."); }
        }
    }
}
