using System;
using System.IO;
using System.Reflection;
using Assets.Behaviour.Flow;
using Assets.Behaviour.UI;
using Assets.Behaviour.UI.BuyMenu;
using Assets.Behaviour.Util;
using Assets.Source.Player;
using Behaviour.UI.Tooltip;
using BepInEx;
using UnityEngine;
using UnityEngine.UI;
using GameInput = Assets.Behaviour.Input.PlayerInput;
using HarmonyLib;
using KingdomFoundation.Api;

namespace SharableBlueprints
{
    public sealed partial class SharableBlueprintsPlugin : BaseUnityPlugin
    {
        void Attach(GameUI ui)
        {
            CleanupUi();
            var original = Field<GameObject>(ui, "_cloneMenuButton");
            var bar = Field<RectTransform>(ui, "_editHotbar");
            var originalArea = Field<AreaCopyGhost>(ui, "_cloneGhost");
            toolbarButton = Instantiate(original, bar, false);
            toolbarButton.name = "SharableBlueprintsButton";
            var image = toolbarButton.transform.Find("Image")?.GetComponent<Image>();
            if (!image) throw new MissingMemberException("The native toolbar icon was not found.");
            image.sprite = icon; image.type = Image.Type.Simple; image.preserveAspect = true;
            image.color = Color.white;
            var nativeBinding = toolbarButton.GetComponent<KeyBindingTooltip>();
            if (nativeBinding) Destroy(nativeBinding);
            var tutorial = toolbarButton.transform.Find("TutorialArrow");
            if (tutorial) Destroy(tutorial.gameObject);
            activeBorder = toolbarButton.transform.Find("Active")?.gameObject;
            if (activeBorder) activeBorder.SetActive(false);
            var tooltip = toolbarButton.GetComponent<TooltipSource>();
            if (tooltip) tooltip.SetTooltip("Sharable Blueprints", "Open your blueprint library and books. Choose New Blueprint to capture an area of your workshop.");
            var selectorObject = new GameObject("SharableBlueprintArea", typeof(RectTransform));
            selectorObject.transform.SetParent(originalArea.transform.parent, false);
            selection = selectorObject.AddComponent<AreaSelection>();
            var canvas = original.GetComponentInParent<Canvas>().rootCanvas;
            var summaryObject = new GameObject("SharableBlueprintSelectionCounts", typeof(RectTransform));
            summary = summaryObject.AddComponent<SelectionSummaryPanel>();
            summary.Initialize(ui, canvas);
            selection.Initialize(this, summary, (RectTransform)originalArea.transform);
            var libraryObject = new GameObject("SharableBlueprintLibrary", typeof(RectTransform));
            library = libraryObject.AddComponent<BlueprintLibraryPopup>();
            library.Initialize(this, ui, canvas.transform);
            var button = toolbarButton.GetComponent<Button>();
            // Replacing the event removes the native button's serialized Clone action too.
            button.onClick = new Button.ButtonClickedEvent();
            button.onClick.AddListener(ToggleLibrary);
            attachedUi = ui;
            toolbarButton.SetActive(true);
            ui.UpdateBuildBarButtons();
            LayoutRebuilder.ForceRebuildLayoutImmediate(bar);
            Logger.LogInfo("Blueprint schematic button attached to the bottom-left edit toolbar.");
        }
    }
}
