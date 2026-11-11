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
        void CopyToClipboard()
        {
            Run(() => {
                var entry = SelectedEntry(); if (entry == null) return;
                var blueprint = ReadBlueprint(entry);
                blueprint.Document["name"] = entry.Name; blueprint.Document["icon"] = entry.IconName;
                BlueprintIdentity.WriteSuggestions(blueprint.Document, entry.Suggestions);
                string code = BlueprintShareCode.EncodeCompact(blueprint.Document.ToString(false));
                var parts = BlueprintShareCode.ForDiscord(code);
                if (parts.Count == 1)
                {
                    GUIUtility.systemCopyBuffer = "```\n" + code + "\n```";
                    SetStatus("Compact code copied: " + code.Length + " characters — fits one Discord message.");
                }
                else ShowDiscordParts(code, parts);
            });
        }
    }
}
