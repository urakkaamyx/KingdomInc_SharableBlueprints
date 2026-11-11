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
        Button ButtonAt(Transform parent, string text, float x, float y, float w, float h, Action action)
        {
            var rect = Rect(parent, "Button", x, y, w, h);
            rect.gameObject.AddComponent<Image>();
            var button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = rect.GetComponent<Image>();
            theme.Button(button);
            float inset = w < 60 ? 8 : w < 120 ? 12 : 18;
            var label = Label(rect, text, inset, 0, w - inset * 2, h, 16, Paper);
            label.alignment = TextAlignmentOptions.Midline;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.enableAutoSizing = true; label.fontSizeMin = 14; label.fontSizeMax = 16;
            if (action != null) button.onClick.AddListener(() => action()); else button.interactable = false;
            return button;
        }
    }
}
