using KingdomFoundation.Api;
using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace SharableBlueprints
{
    public sealed class SelectionSummaryPanel : MonoBehaviour
    {
        RectTransform panel, viewport, content, nameColumn;
        TMP_Text total, names, counts, hint;
        FoundationUiInputBlocker blocker;
        Canvas parentCanvas;
        float scrollOffset;
        public void Initialize(Component ui, Canvas canvas)
        {
            blocker = gameObject.AddComponent<FoundationUiInputBlocker>();
            parentCanvas = canvas;
            transform.SetParent(canvas.transform, false);
            var root = (RectTransform)transform;
            root.anchorMin = Vector2.zero;
            root.anchorMax = Vector2.one;
            root.offsetMin = root.offsetMax = Vector2.zero;
            var overlay = gameObject.AddComponent<Canvas>();
            overlay.overrideSorting = true;
            overlay.sortingOrder = 80;
            var passThrough = gameObject.AddComponent<CanvasGroup>();
            passThrough.blocksRaycasts = passThrough.interactable = false;
            panel = Rect(root, "BuildingCounts", 0, 0, 280, 240);
            panel.anchorMin = panel.anchorMax = panel.pivot = new Vector2(0, .5f);
            panel.anchoredPosition = new Vector2(12, 0);
            var background = panel.gameObject.AddComponent<Image>();
            background.raycastTarget = false;
            new BlueprintUiTheme().Image(background, "Tooltip", new Color(.13f, .105f, .09f, .95f));
            var font = ui.GetComponentsInChildren<TMP_Text>(true).Select(t => t.font).First(f => f);
            Text(panel, font, "SELECTED BUILDINGS", 14, 12, 252, 24, 18);
            total = Text(panel, font, "0 buildings", 14, 42, 252, 40, 16);
            viewport = Rect(panel, "ListViewport", 14, 94, 252, 84);
            viewport.gameObject.AddComponent<RectMask2D>();
            content = Rect(viewport, "Content", 0, 0, 252, 84);
            nameColumn = Rect(content, "Names", 0, 0, 180, 84);
            nameColumn.gameObject.AddComponent<RectMask2D>();
            names = Text(nameColumn, font, "", 0, 0, 180, 84, 16);
            counts = Text(content, font, "", 188, 0, 64, 84, 16);
            counts.alignment = TextAlignmentOptions.TopRight;
            hint = Text(panel, font, "Drag an area · Escape cancels", 14, 188, 252, 38, 13);
            gameObject.SetActive(false);
        }

        public void Hide()
        {
            scrollOffset = 0;
            gameObject.SetActive(false);
        }

        void LateUpdate()
        {
            Layout();
            var mouse = Mouse.current;
            bool hovered = Application.isFocused && mouse != null && RectTransformUtility.RectangleContainsScreenPoint(panel, mouse.position.ReadValue(), parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : parentCanvas.worldCamera);
            // A pass-through panel must not intercept the selection or stop left-edge panning.
            // Manually block native zoom while the wheel is scrolling this list instead.
            if (hovered)
            {
                blocker.OnPointerEnter(null);
                scrollOffset -= mouse.scroll.ReadValue().y * .3f;
                Layout();
            }
            else
                blocker?.OnPointerExit(null);
        }

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
            hint.text = content.rect.height > viewport.rect.height + 1 ? "Wheel over this list to scroll\nRelease to save · Escape cancels" : "Hold at screen edge to pan\nRelease to save · Escape cancels";
        }

        void OnDisable()
        {
            blocker?.OnPointerExit(null);
        }

        static RectTransform Rect(Transform parent, string name, float x, float y, float width, float height)
        {
            var rect = (RectTransform)new GameObject(name, typeof(RectTransform)).transform;
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(x, -y);
            rect.sizeDelta = new Vector2(width, height);
            return rect;
        }

        public void Show(SelectionPreview preview, bool dragging)
        {
            gameObject.SetActive(true);
            total.text = preview.Buildings + " buildings · " + preview.Nodes + " nodes\n" + preview.Containers + " containers · " + preview.Decorations + " labels/icons";
            string labels = preview.Groups.Count == 0 ? (dragging ? "No complete buildings yet" : "Drag around your buildings") : string.Join("\n", preview.Groups.Select(g => g.Name));
            string values = string.Join("\n", preview.Groups.Select(g => "× " + g.Buildings));
            if (names.text != labels)
                names.text = labels;
            if (counts.text != values)
                counts.text = values;
            Layout();
        }

        static TMP_Text Text(Transform parent, TMP_FontAsset font, string value, float x, float y, float width, float height, float size)
        {
            var text = Rect(parent, "Text", x, y, width, height).gameObject.AddComponent<TextMeshProUGUI>();
            text.font = font;
            text.fontSize = size;
            text.text = value;
            text.richText = false;
            text.raycastTarget = false;
            text.color = new Color(.91f, .84f, .66f);
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.overflowMode = TextOverflowModes.Overflow;
            return text;
        }
    }
}
