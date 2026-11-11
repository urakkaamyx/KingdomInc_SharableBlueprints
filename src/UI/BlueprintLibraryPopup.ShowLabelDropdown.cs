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
        void ShowLabelDropdown()
        {
            var entry = SelectedEntry();
            if (entry == null || entry.Suggestions.Labels.Count == 0 || picker) return;
            var overlay = PickerOverlay("LabelDropdown");
            float height = Mathf.Min(300, entry.Suggestions.Labels.Count * 54 + 12);
            var box = Box(overlay, "Labels", 682, 302, 434, height, Wood);
            box.gameObject.AddComponent<Button>(); // Swallow clicks in the dropdown's padding.
            var rows = Scroll(box, "LabelChoices", 8, 8, 418, height - 16);
            foreach (string name in entry.Suggestions.Labels)
                Row(rows, name, () => { blueprintName.SetTextWithoutNotify(name); DismissPicker(); }, blueprintName.text == name);
        }
    }
}
