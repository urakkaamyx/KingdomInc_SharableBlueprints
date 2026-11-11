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
        void Update()
        {
            var ui = GameUI.Instance;
            if (!ui || FoundationApi.World.Player == null)
            {
                if (toolbarButton || selection) CleanupUi();
                return;
            }
            if (ui != attachedUi || !toolbarButton)
            {
                if (Time.unscaledTime < nextAttach) return;
                nextAttach = Time.unscaledTime + 2;
                try { Attach(ui); }
                catch (Exception error) { CleanupUi(); Logger.LogError("Blueprint toolbar: " + error); }
            }
            if (toolbarButton)
            {
                toolbarButton.SetActive(WorldFlowManager.Visible);
                toolbarButton.GetComponent<Button>().interactable = WorldFlowManager.Visible && !GameUI.MenuVisible;
            }
            if (selection && selection.Active && !selection.ContextValid) selection.Cancel();
            if (library && library.IsOpen && (!WorldFlowManager.Visible || GameUI.MenuVisible)) library.ClosePopup();
            SetSelected((selection && selection.Active) || (library && library.IsOpen));
        }
    }
}
