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
        TMP_InputField Input(Transform parent, string placeholder, float x, float y, float w, float h)
        {
            var rect = Box(parent, "Input", x, y, w, h, Ink);
            var viewport = Rect(rect, "Viewport", 12, 6, w - 24, h - 12); viewport.gameObject.AddComponent<RectMask2D>();
            var text = (TextMeshProUGUI)Label(viewport, "", 0, 0, w - 24, h - 12, 16, Paper);
            text.alignment = TextAlignmentOptions.MidlineLeft; text.textWrappingMode = TextWrappingModes.NoWrap;
            text.overflowMode = TextOverflowModes.Overflow;
            var hint = Label(viewport, placeholder, 0, 0, w - 24, h - 12, 16, new Color(.67f, .62f, .52f));
            hint.alignment = TextAlignmentOptions.MidlineLeft;
            hint.textWrappingMode = TextWrappingModes.NoWrap;
            var field = rect.gameObject.AddComponent<TMP_InputField>();
            field.targetGraphic = rect.GetComponent<Image>(); field.textViewport = viewport;
            field.textComponent = text; field.placeholder = hint; field.characterLimit = 80;
            field.lineType = TMP_InputField.LineType.SingleLine; field.richText = false;
            field.customCaretColor = true; field.caretColor = Paper; field.caretWidth = 2;
            return field;
        }
    }
}
