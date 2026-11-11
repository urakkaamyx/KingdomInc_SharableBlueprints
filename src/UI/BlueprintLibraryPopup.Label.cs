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
        TMP_Text Label(Transform parent, string text, float x, float y, float w, float h, float size, Color color)
        {
            var label = Rect(parent, "Label", x, y, w, h).gameObject.AddComponent<TextMeshProUGUI>();
            label.font = font; label.fontSize = size; label.text = text; label.color = color;
            label.richText = false; label.raycastTarget = false; label.overflowMode = TextOverflowModes.Ellipsis;
            label.textWrappingMode = TextWrappingModes.Normal;
            return label;
        }
    }
}
