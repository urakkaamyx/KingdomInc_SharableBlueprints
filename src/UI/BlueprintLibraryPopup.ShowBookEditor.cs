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
        void ShowBookEditor(bool create)
        {
            if (picker) return;
            var book = store.Books.FirstOrDefault(b => b.Id == bookFilter);
            if (!create && book == null) return;
            var overlay = PickerOverlay("BookEditor");
            var box = Dialog(overlay, "Book", 540, create ? 296 : 356);
            box.gameObject.AddComponent<Button>();
            Label(box, create ? "NEW BLUEPRINT BOOK" : "MANAGE BLUEPRINT BOOK", 24, 22, 492, 30, 21, Paper);
            Label(box, create ? "Give your collection a name." : "Rename this book or remove it from your library.", 24, 66, 492, 36, 16, Paper);
            var name = Input(box, "Book name", 24, 116, 492, 44);
            name.SetTextWithoutNotify(create ? "" : book.Name);
            var errorLabel = Label(box, "", 24, 174, 492, 42, 14, Paper);
            var save = ButtonAt(box, create ? "Create Book" : "Save Name", 24, 230, 236, 42, () => {
                try
                {
                    if (create) bookFilter = store.CreateBook(name.text).Id;
                    else store.RenameBook(book.Id, name.text);
                    DismissPicker(); Refresh();
                    SetStatus(create ? "Book created. New blueprints will be stored here." : "Book renamed.");
                }
                catch (Exception error) { errorLabel.text = error.Message; owner.LogLibraryError(error); }
            });
            theme.Button(save, true);
            ButtonAt(box, "Cancel", 280, 230, 236, 42, DismissPicker);
            if (!create)
            {
                var remove = ButtonAt(box, "Delete Book", 24, 294, 492, 42, () => { DismissPicker(); ConfirmDeleteBook(); });
                theme.Button(remove, false, true);
            }
        }
    }
}
