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
        void BookRow(string name, string id, int count)
        {
            var button = Row(bookRows, name, () => { bookFilter = id; Refresh(); }, bookFilter == id);
            var title = button.GetComponentInChildren<TMP_Text>();
            title.rectTransform.anchoredPosition = new Vector2(18, 0);
            title.rectTransform.sizeDelta = new Vector2(((RectTransform)button.transform).sizeDelta.x - 60, 54);
            var total = Label(button.transform, count.ToString(), ((RectTransform)button.transform).sizeDelta.x - 40, 0, 24, 54, 15, Paper);
            total.enableAutoSizing = true; total.fontSizeMin = 12; total.fontSizeMax = 15;
            total.alignment = TextAlignmentOptions.MidlineRight;
        }
    }
}
