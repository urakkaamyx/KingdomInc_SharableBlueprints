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
        RectTransform Scroll(Transform parent, string name, float x, float y, float w, float h)
        {
            var rect = Rect(parent, name, x, y, w, h);
            var scroll = rect.gameObject.AddComponent<ScrollRect>();
            var viewport = Rect(rect, "Viewport", 0, 0, w - 24, h); viewport.gameObject.AddComponent<RectMask2D>();
            var hit = viewport.gameObject.AddComponent<Image>(); hit.color = new Color(0, 0, 0, .01f);
            var content = Rect(viewport, "Content", 0, 0, w - 24, 0);
            var layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(3, 3, 3, 3); layout.spacing = 8;
            layout.childControlWidth = layout.childControlHeight = true;
            layout.childForceExpandWidth = true; layout.childForceExpandHeight = false;
            content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.viewport = viewport; scroll.content = content; scroll.horizontal = false; scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped; scroll.scrollSensitivity = 30;
            AddScrollbar(rect, scroll, w, h);
            return content;
        }
    }
}
