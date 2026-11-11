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
        void AddScrollbar(RectTransform parent, ScrollRect scroll, float width, float height)
        {
            var track = Rect(parent, "Scrollbar", width - 18, 0, 18, height);
            theme.Image(track.gameObject.AddComponent<Image>(), "ScrollbarBG", Ink);
            var slide = Rect(track, "SlidingArea", 3, 10, 12, height - 20);
            var handle = Rect(slide, "Handle", 0, 0, 12, 24);
            handle.anchorMin = Vector2.zero; handle.anchorMax = Vector2.one;
            handle.offsetMin = handle.offsetMax = Vector2.zero;
            var graphic = handle.gameObject.AddComponent<Image>();
            theme.Image(graphic, "ScrollHandle", Gold);
            var bar = track.gameObject.AddComponent<Scrollbar>();
            bar.handleRect = handle; bar.targetGraphic = graphic;
            bar.direction = Scrollbar.Direction.BottomToTop;
            scroll.verticalScrollbar = bar;
        }
    }
}
