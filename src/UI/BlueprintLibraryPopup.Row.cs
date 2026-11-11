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
        Button Row(Transform parent, string text, Action action, bool selected)
        {
            var button = ButtonAt(parent, text, 0, 0, ((RectTransform)parent).sizeDelta.x - 6, 54, action);
            button.gameObject.AddComponent<LayoutElement>().preferredHeight = 54;
            button.GetComponentInChildren<TMP_Text>().alignment = TextAlignmentOptions.MidlineLeft;
            theme.Button(button, selected);
            if (!string.IsNullOrEmpty(text)) button.gameObject.AddComponent<TooltipSource>().SetTooltip(text, "");
            return button;
        }
    }
}
