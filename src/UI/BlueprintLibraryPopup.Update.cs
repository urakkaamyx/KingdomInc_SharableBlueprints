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
        void Update()
        {
            // Native menus suspend game shortcuts while typing; keep mouse UI working.
            if (Time.frameCount == openedFrame) return;
            if ((Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                || (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame))
            {
                if (picker) DismissPicker();
                else ClosePopup();
            }
            var parent = transform.parent as RectTransform;
            if (parent)
            {
                float scale = Mathf.Min(1, (parent.rect.width - 32) / PanelWidth, (parent.rect.height - 32) / PanelHeight);
                panel.localScale = Vector3.one * Mathf.Max(.1f, scale);
            }
        }
    }
}
