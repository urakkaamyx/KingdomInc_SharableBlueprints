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
        public void ShowResearch(BlueprintResearchReport report)
        {
            DismissPicker();
            var overlay = PickerOverlay("BlueprintResearch");
            var box = Dialog(overlay, "Research", 668, 506); box.gameObject.AddComponent<Button>();
            Label(box, report.CanPlace ? "RESEARCH READY" : "CANNOT PLACE — RESEARCH REQUIRED", 18, 14, 632, 32, 21, Paper);
            Label(box, "You can still paste, store, organize and share this blueprint.", 18, 54, 632, 30, 15, Paper);
            var rows = Scroll(box, "Requirements", 16, 98, 636, 344);
            if (report.CanPlace) Row(rows, "All required research is available in this kingdom.", null, false);
            foreach (var issue in report.Issues)
            {
                string message = issue.Name + "\n" + issue.Description + "\nNeeded by: " + string.Join(", ", issue.Affected.OrderBy(s => s));
                float width = rows.sizeDelta.x - 6;
                var label = Label(rows, message, 8, 0, width, 100, 16, Paper);
                label.margin = new Vector4(8, 8, 8, 8); label.textWrappingMode = TextWrappingModes.Normal;
                label.gameObject.AddComponent<LayoutElement>().preferredHeight = Mathf.Max(96, label.GetPreferredValues(message, width, float.PositiveInfinity).y + 24);
            }
            ButtonAt(box, "Back to Library", 18, 460, 632, 30, DismissPicker);
        }
    }
}
