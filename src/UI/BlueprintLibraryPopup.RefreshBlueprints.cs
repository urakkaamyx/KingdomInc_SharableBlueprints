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
        void RefreshBlueprints()
        {
            Clear(blueprintRows);
            var matches = store.Blueprints.Where(b => (bookFilter == "*" || b.BookId == bookFilter)
                && b.Name.IndexOf(search.text ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                .OrderBy(b => b.Name, StringComparer.OrdinalIgnoreCase).ToList();
            foreach (var entry in matches)
            {
                var row = Row(blueprintRows, entry.Name + (File.Exists(store.FilePath(entry)) ? "" : " [file missing]"),
                    () => SelectBlueprint(entry), selectedId == entry.Id);
                var title = row.GetComponentInChildren<TMP_Text>();
                title.rectTransform.anchoredPosition = new Vector2(62, 0);
                title.rectTransform.sizeDelta = new Vector2(((RectTransform)row.transform).sizeDelta.x - 82, 54);
                IconAt(row.transform, BlueprintIdentity.Sprite(entry.IconName) ?? owner.BlueprintIcon, 20, 11, 32);
            }
            if (matches.Count == 0) Row(blueprintRows, "No blueprints here yet", null, false);
            if (!matches.Any(b => b.Id == selectedId)) selectedId = null;
            ShowDetails();
        }
    }
}
