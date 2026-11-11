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
        void ConfirmDeleteBlueprint()
        {
            var entry = SelectedEntry(); if (entry == null || picker) return;
            ConfirmDelete("DELETE BLUEPRINT", entry.Name,
                "Remove this blueprint from the library?\nA recoverable copy will be kept locally.", () => {
                    store.DeleteBlueprint(entry.Id); selectedId = editingId = null;
                }, "Blueprint deleted. A recoverable copy is kept in the library's _Deleted folder.");
        }
    }
}
