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
        void ShowIconPicker()
        {
            var entry = SelectedEntry();
            if (entry == null || picker) return;
            var overlay = PickerOverlay("IconPicker");
            var box = Dialog(overlay, "Icons", 640, 472); box.gameObject.AddComponent<Button>();
            Label(box, "CHOOSE BLUEPRINT ICON", 18, 14, 600, 30, 22, Paper);
            var fromSelection = ButtonAt(box, "From Selection (" + entry.Suggestions.Icons.Count + ")", 16, 56, 300, 32, null);
            var allIcons = ButtonAt(box, "All Game Icons", 324, 56, 300, 32, null);
            var filter = Input(box, "Search icons…", 16, 100, 608, 32);
            var grid = IconGrid(box, 16, 144, 608, 264);
            var empty = Label(box, "", 24, 154, 590, 42, 16, Paper);
            bool selectionOnly = entry.Suggestions.Icons.Count > 0;
            var catalog = BlueprintIdentity.Catalog();
            Action redraw = () => {
                Clear(grid);
                var options = selectionOnly ? entry.Suggestions.Icons : catalog;
                var available = options.Where(i => (i.Name ?? i.Id).IndexOf(filter.text ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    || i.Id.IndexOf(filter.text ?? "", StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                empty.text = available.Count == 0 ? "No matching icons." : "";
                theme.Button(fromSelection, selectionOnly);
                theme.Button(allIcons, !selectionOnly);
                foreach (var option in available)
                {
                    Sprite sprite = BlueprintIdentity.Sprite(option.Id);
                    if (!sprite) continue;
                    var button = ButtonAt(grid, "", 0, 0, 66, 66, () => {
                        draftIconName = option.Id; selectedIcon.sprite = sprite; DismissPicker();
                        SetStatus("Icon selected. Save Name & Icon to keep your changes.");
                    });
                    IconAt(button.transform, sprite, 13, 13, 40);
                    theme.Button(button, draftIconName == option.Id);
                    button.gameObject.AddComponent<TooltipSource>().SetTooltip(option.Name, "Use this blueprint icon.");
                }
            };
            fromSelection.interactable = allIcons.interactable = true;
            fromSelection.onClick.AddListener(() => { selectionOnly = true; redraw(); });
            allIcons.onClick.AddListener(() => { selectionOnly = false; redraw(); });
            filter.onValueChanged.AddListener(_ => redraw());
            ButtonAt(box, "Use Schematic", 16, 424, 220, 32, () => {
                draftIconName = ""; selectedIcon.sprite = owner.BlueprintIcon; DismissPicker();
                SetStatus("Schematic icon selected. Save Name & Icon to keep it.");
            });
            ButtonAt(box, "Cancel", 404, 424, 220, 32, DismissPicker);
            redraw();
        }
    }
}
