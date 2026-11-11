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
        void ShowDiscordParts(string code, System.Collections.Generic.List<string> parts)
        {
            DismissPicker(); var overlay = PickerOverlay("DiscordShare");
            var box = Dialog(overlay, "Share", 668, 472); box.gameObject.AddComponent<Button>();
            Label(box, "SHARE BLUEPRINT IN DISCORD", 18, 14, 632, 30, 22, Paper);
            Label(box, code.Length + " characters · " + parts.Count + " messages\nCopy and send each part. The recipient pastes all parts together.", 18, 58, 632, 62, 16, Paper);
            var rows = Scroll(box, "Parts", 16, 132, 636, 258);
            for (int i = 0; i < parts.Count; i++)
            {
                int index = i;
                Row(rows, "Copy Part " + (i + 1) + " / " + parts.Count + "  (" + parts[i].Length + " characters)", () => {
                    GUIUtility.systemCopyBuffer = "```\n" + parts[index] + "\n```"; SetStatus("Discord part " + (index + 1) + " copied.");
                }, false);
            }
            ButtonAt(box, "Copy Whole Code", 18, 420, 300, 32, () => { GUIUtility.systemCopyBuffer = "```\n" + code + "\n```"; SetStatus("Whole compact blueprint code copied."); });
            ButtonAt(box, "Done", 350, 420, 300, 32, DismissPicker);
        }
    }
}
