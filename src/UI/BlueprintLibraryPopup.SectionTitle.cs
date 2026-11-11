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
        void SectionTitle(Transform parent, string text, float x, float y, float w)
        {
            var bar = Rect(parent, "SectionTitle", x, y, w, 32);
            theme.Image(bar.gameObject.AddComponent<Image>(), "TitleBlue", Wood);
            var label = Label(bar, text, 12, 0, w - 24, 32, 16, Paper);
            label.alignment = TextAlignmentOptions.MidlineLeft;
        }
    }
}
