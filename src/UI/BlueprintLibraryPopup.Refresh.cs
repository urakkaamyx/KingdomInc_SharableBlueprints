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
        void Refresh()
        {
            Clear(bookRows);
            BookRow("All Blueprints", "*", store.Blueprints.Count);
            BookRow("Unfiled", "", store.Blueprints.Count(b => b.BookId == ""));
            foreach (var book in store.Books.OrderBy(b => b.Name, StringComparer.OrdinalIgnoreCase))
                BookRow(book.Name, book.Id, store.Blueprints.Count(b => b.BookId == book.Id));
            var selectedBook = store.Books.FirstOrDefault(b => b.Id == bookFilter);
            manageBook.interactable = selectedBook != null;
            RefreshBlueprints();
        }
    }
}
