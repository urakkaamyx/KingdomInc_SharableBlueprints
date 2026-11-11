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
        RectTransform PickerOverlay(string name)
        {
            picker = Rect(panel, name, 0, 0, PanelWidth, PanelHeight);
            var shade = picker.gameObject.AddComponent<Image>(); shade.color = new Color(0, 0, 0, .45f);
            var dismiss = picker.gameObject.AddComponent<Button>(); dismiss.targetGraphic = shade; dismiss.onClick.AddListener(DismissPicker);
            return picker;
        }
    }
}
