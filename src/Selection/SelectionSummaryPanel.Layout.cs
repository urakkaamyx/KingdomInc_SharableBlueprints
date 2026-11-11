using System;
using System.Linq;
using Assets.Behaviour.UI;
using Assets.Behaviour.UI.Shared;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace SharableBlueprints
{
    public sealed partial class SelectionSummaryPanel : UIBlocksZoom
    {
        void Layout()
        {
            var parent = transform.parent as RectTransform;
            float maxHeight = parent ? Mathf.Max(180, parent.rect.height - 190) : 600;
            float textHeight = Mathf.Max(32, names.preferredHeight, counts.preferredHeight);
            float height = Mathf.Min(Mathf.Max(214, textHeight + 156), maxHeight);
            panel.sizeDelta = new Vector2(280, height);
            viewport.sizeDelta = new Vector2(252, Mathf.Max(32, height - 152));
            content.sizeDelta = new Vector2(252, Mathf.Max(textHeight, viewport.rect.height));
            nameColumn.sizeDelta = new Vector2(180, content.rect.height);
            names.rectTransform.sizeDelta = new Vector2(180, content.rect.height);
            counts.rectTransform.sizeDelta = new Vector2(64, content.rect.height);
            scrollOffset = Mathf.Clamp(scrollOffset, 0, Mathf.Max(0, content.rect.height - viewport.rect.height));
            content.anchoredPosition = new Vector2(0, scrollOffset);
            hint.rectTransform.anchoredPosition = new Vector2(14, -(height - 46));
            hint.text = content.rect.height > viewport.rect.height + 1
                ? "Wheel over this list to scroll\nRelease to save · Escape cancels"
                : "Hold at screen edge to pan\nRelease to save · Escape cancels";
        }
    }
}
