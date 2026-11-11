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
        void ShowPasteDialog()
        {
            if (picker) return;
            var overlay = PickerOverlay("PasteBlueprint");
            var box = Dialog(overlay, "Paste", 668, 472); box.gameObject.AddComponent<Button>();
            Label(box, "PASTE INTO BLUEPRINT LIBRARY", 18, 14, 632, 30, 22, Paper);
            Label(box, "Locked buildings can be stored. Research is checked when placing.", 18, 54, 632, 42, 15, Paper);
            var text = Input(box, "Paste a compact code, all Discord parts, or blueprint JSON here…", 18, 106, 632, 268);
            text.characterLimit = 0; text.lineType = TMP_InputField.LineType.MultiLineNewline;
            text.textComponent.textWrappingMode = TextWrappingModes.Normal; text.textComponent.alignment = TextAlignmentOptions.TopLeft;
            text.textComponent.fontSize = 13;
            text.SetTextWithoutNotify(GUIUtility.systemCopyBuffer ?? "");
            var errorLabel = Label(box, "", 18, 380, 632, 36, 14, Paper);
            ButtonAt(box, "Store Blueprint", 18, 426, 300, 30, () => {
                try
                {
                    var blueprint = PortableBlueprint.Parse(text.text);
                    var suggestions = BlueprintIdentity.FromDocument(blueprint.Document);
                    blueprint.Document["name"] = blueprint.Name;
                    var entry = store.Add(blueprint.Document.ToString(true), blueprint.Name, CaptureBookId,
                        blueprint.Buildings, blueprint.Pipes, blueprint.Decorations, suggestions, blueprint.Icon);
                    DismissPicker(); Open(entry.Id, "Blueprint stored. Locked content stays in your library until you research it.");
                    Run(() => { var report = BlueprintResearch.Check(blueprint); if (!report.CanPlace) ShowResearch(report); });
                }
                catch (Exception error) { errorLabel.text = error.Message; owner.LogLibraryError(error); }
            });
            ButtonAt(box, "Cancel", 350, 426, 300, 30, DismissPicker);
        }
    }
}
