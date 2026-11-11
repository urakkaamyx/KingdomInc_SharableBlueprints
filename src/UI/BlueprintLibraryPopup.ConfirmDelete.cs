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
        void ConfirmDelete(string title, string name, string message, Action action, string success)
        {
            var overlay = PickerOverlay("ConfirmDelete");
            var box = Dialog(overlay, "Confirm", 540, 308); box.gameObject.AddComponent<Button>();
            Label(box, title, 18, 14, 504, 28, 21, Paper);
            Label(box, name, 18, 54, 504, 66, 21, Paper);
            Label(box, message, 18, 128, 504, 66, 16, Paper);
            var errorLabel = Label(box, "", 18, 198, 504, 40, 14, Paper);
            var confirm = ButtonAt(box, "Delete", 18, 256, 242, 34, () => {
                try { action(); DismissPicker(); Refresh(); SetStatus(success); }
                catch (Exception error) { errorLabel.text = error.Message; owner.LogLibraryError(error); }
            });
            theme.Button(confirm, false, true);
            ButtonAt(box, "Cancel", 280, 256, 242, 34, DismissPicker);
        }
    }
}
