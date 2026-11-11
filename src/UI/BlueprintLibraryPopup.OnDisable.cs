using System;
using System.IO;
using System.Linq;
using Behaviour.UI.Tooltip;
using Assets.Behaviour.UI;
using Assets.Behaviour.UI.Shared;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using GameInput = Assets.Behaviour.Input.PlayerInput;

namespace SharableBlueprints
{
    public sealed partial class BlueprintLibraryPopup : UIPopupMenu
    {
        void OnDisable()
        {
            // Also release the native zoom blocker when deactivated outside TogglePopupMenu.
            base.OnPointerExit(null);
            if (restoreInput && GameUI.Instance && !GameUI.MenuVisible) GameInput.Enable();
            restoreInput = false;
        }
    }
}
