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
        static TMP_Text Text(Transform parent, TMP_FontAsset font, string value, float x, float y, float width, float height, float size)
        {
            var text = Rect(parent, "Text", x, y, width, height).gameObject.AddComponent<TextMeshProUGUI>();
            text.font = font; text.fontSize = size; text.text = value; text.richText = false;
            text.raycastTarget = false; text.color = new Color(.91f, .84f, .66f);
            text.textWrappingMode = TextWrappingModes.NoWrap; text.overflowMode = TextOverflowModes.Overflow;
            return text;
        }
    }
}
