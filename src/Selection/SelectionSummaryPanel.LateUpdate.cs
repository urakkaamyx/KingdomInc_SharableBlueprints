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
        void LateUpdate()
        {
            Layout();
            var mouse = Mouse.current;
            bool hovered = Application.isFocused && mouse != null && RectTransformUtility.RectangleContainsScreenPoint(panel,
                mouse.position.ReadValue(), parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : parentCanvas.worldCamera);
            // A pass-through panel must not intercept the selection or stop left-edge panning.
            // Manually block native zoom while the wheel is scrolling this list instead.
            if (hovered)
            {
                base.OnPointerEnter(null);
                scrollOffset -= mouse.scroll.ReadValue().y * .3f;
                Layout();
            }
            else base.OnPointerExit(null);
        }
    }
}
