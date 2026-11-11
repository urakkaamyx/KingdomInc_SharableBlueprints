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
        void ToggleLibrary()
        {
            if (!library || !WorldFlowManager.Visible) return;
            if (library.IsOpen) { library.ClosePopup(); return; }
            if (selection && selection.Active) selection.Cancel();
            DragDropManager.Instance.CancelDragDrop();
            library.Open();
        }
    }
}
