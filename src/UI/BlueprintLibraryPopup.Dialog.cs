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
        RectTransform Dialog(Transform parent, string name, float w, float h)
        {
            var box = Box(parent, name, 0, 0, w, h, Wood);
            box.anchorMin = box.anchorMax = box.pivot = new Vector2(.5f, .5f);
            box.anchoredPosition = Vector2.zero;
            return box;
        }
    }
}
