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
        void ShowBookPicker()
        {
            if (selectedId == null || picker) return;
            var overlay = PickerOverlay("BookPicker");
            var box = Dialog(overlay, "ChooseBook", 452, 456); box.gameObject.AddComponent<Button>();
            Label(box, "MOVE BLUEPRINT TO BOOK", 18, 16, 416, 30, 20, Paper);
            var rows = Scroll(box, "Destinations", 14, 60, 424, 328);
            Action<string> move = id => Run(() => {
                store.MoveBlueprint(selectedId, id); bookFilter = id;
                DismissPicker(); Refresh(); SetStatus("Blueprint moved.");
            });
            Row(rows, "Unfiled", () => move(""), false);
            foreach (var book in store.Books.OrderBy(b => b.Name, StringComparer.OrdinalIgnoreCase))
                Row(rows, book.Name, () => move(book.Id), false);
            ButtonAt(box, "Cancel", 14, 404, 424, 36, DismissPicker);
        }
    }
}
