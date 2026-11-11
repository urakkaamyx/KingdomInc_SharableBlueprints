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
        public void Open(string select = null, string message = null)
        {
            try
            {
                store.Load();
                if (bookFilter != "*" && bookFilter != "" && !store.Books.Any(b => b.Id == bookFilter)) bookFilter = "*";
                if (select != null)
                {
                    editingId = null;
                    selectedId = select;
                    var entry = store.Blueprints.FirstOrDefault(b => b.Id == select);
                    if (entry != null) bookFilter = entry.BookId;
                    search.SetTextWithoutNotify("");
                }
                Refresh();
                SetStatus(message ?? "Choose a book or select an area with New Blueprint.");
                GameUI.Instance.TogglePopupMenu(this, true);
                transform.SetAsLastSibling();
                openedFrame = Time.frameCount;
                restoreInput = GameInput.Enabled;
                GameInput.Disable();
            }
            catch (Exception error) { owner.ReportLibraryError(error); }
        }
    }
}
