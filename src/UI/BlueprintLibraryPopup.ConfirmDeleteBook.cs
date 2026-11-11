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
        void ConfirmDeleteBook()
        {
            var book = store.Books.FirstOrDefault(b => b.Id == bookFilter); if (book == null || picker) return;
            int count = store.Blueprints.Count(b => b.BookId == book.Id);
            ConfirmDelete("DELETE BLUEPRINT BOOK", book.Name,
                "Remove this book? Its " + count + " blueprint(s) will be kept in Unfiled.", () => {
                    store.DeleteBook(book.Id); bookFilter = "";
                }, "Book deleted. Its blueprints are kept in Unfiled.");
        }
    }
}
