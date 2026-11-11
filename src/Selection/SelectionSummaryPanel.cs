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
        RectTransform panel, viewport, content, nameColumn;
        TMP_Text total, names, counts, hint;
        Canvas parentCanvas;
        float scrollOffset;
        public void Initialize(GameUI ui, Canvas canvas)
        {
            parentCanvas = canvas;
            transform.SetParent(canvas.transform, false);
            var root = (RectTransform)transform;
            root.anchorMin = Vector2.zero; root.anchorMax = Vector2.one;
            root.offsetMin = root.offsetMax = Vector2.zero;
            var overlay = gameObject.AddComponent<Canvas>(); overlay.overrideSorting = true; overlay.sortingOrder = 80;
            var passThrough = gameObject.AddComponent<CanvasGroup>(); passThrough.blocksRaycasts = passThrough.interactable = false;
            panel = Rect(root, "BuildingCounts", 0, 0, 280, 240);
            panel.anchorMin = panel.anchorMax = panel.pivot = new Vector2(0, .5f);
            panel.anchoredPosition = new Vector2(12, 0);
            var background = panel.gameObject.AddComponent<Image>(); background.raycastTarget = false;
            new BlueprintUiTheme().Image(background, "Tooltip", new Color(.13f, .105f, .09f, .95f));
            var font = ui.GetComponentsInChildren<TMP_Text>(true).Select(t => t.font).First(f => f);
            Text(panel, font, "SELECTED BUILDINGS", 14, 12, 252, 24, 18);
            total = Text(panel, font, "0 buildings", 14, 42, 252, 40, 16);
            viewport = Rect(panel, "ListViewport", 14, 94, 252, 84); viewport.gameObject.AddComponent<RectMask2D>();
            content = Rect(viewport, "Content", 0, 0, 252, 84);
            nameColumn = Rect(content, "Names", 0, 0, 180, 84); nameColumn.gameObject.AddComponent<RectMask2D>();
            names = Text(nameColumn, font, "", 0, 0, 180, 84, 16);
            counts = Text(content, font, "", 188, 0, 64, 84, 16); counts.alignment = TextAlignmentOptions.TopRight;
            hint = Text(panel, font, "Drag an area · Escape cancels", 14, 188, 252, 38, 13);
            gameObject.SetActive(false);
        }
    }
}
