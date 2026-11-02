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

namespace SharableBlueprints
{
    [BepInPlugin(Id, "Sharable Blueprints", Version)]
    public sealed class SharableBlueprintsPlugin : BaseUnityPlugin
    {
        public const string Id = "urakka.kingdominc.sharableblueprints", Version = "0.5.2";
        Harmony patches;
        bool placementHooksReady;
        GameUI attachedUi;
        GameObject toolbarButton, activeBorder;
        AreaSelection selection;
        BlueprintLibraryPopup library;
        SelectionSummaryPanel summary;
        string captureBookId;
        Texture2D iconTexture;
        Sprite icon;
        float nextAttach;
        public string ExportDirectory => Path.Combine(Paths.ConfigPath, "SharableBlueprints", "Blueprints");
        internal Sprite BlueprintIcon => icon;

        void Awake()
        {
            patches = new Harmony(Id);
            try { patches.PatchAll(Assembly.GetExecutingAssembly()); placementHooksReady = true; }
            catch (Exception error) { Logger.LogError("Blueprint placement disabled: research guards could not be installed. " + error); }
            using (var input = Assembly.GetExecutingAssembly().GetManifestResourceStream("medieval-schematic.png"))
            using (var bytes = new MemoryStream())
            {
                if (input == null) throw new InvalidOperationException("Missing schematic icon.");
                input.CopyTo(bytes);
                iconTexture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                if (!ImageConversion.LoadImage(iconTexture, bytes.ToArray())) throw new InvalidDataException("Invalid schematic icon.");
                iconTexture.filterMode = FilterMode.Point; iconTexture.wrapMode = TextureWrapMode.Clamp;
                icon = Sprite.Create(iconTexture, new Rect(0, 0, iconTexture.width, iconTexture.height), new Vector2(.5f, .5f), 100);
            }
            Logger.LogInfo("Sharable Blueprints ready. Area exports: " + ExportDirectory);
        }

        void Update()
        {
            var ui = GameUI.Instance;
            if (!ui || GamePlayer.Current == null)
            {
                if (toolbarButton || selection) CleanupUi();
                return;
            }
            if (ui != attachedUi || !toolbarButton)
            {
                if (Time.unscaledTime < nextAttach) return;
                nextAttach = Time.unscaledTime + 2;
                try { Attach(ui); }
                catch (Exception error) { CleanupUi(); Logger.LogError("Blueprint toolbar: " + error); }
            }
            if (toolbarButton)
            {
                toolbarButton.SetActive(WorldFlowManager.Visible);
                toolbarButton.GetComponent<Button>().interactable = WorldFlowManager.Visible && !GameUI.MenuVisible;
            }
            if (selection && selection.Active && !selection.ContextValid) selection.Cancel();
            if (library && library.IsOpen && (!WorldFlowManager.Visible || GameUI.MenuVisible)) library.ClosePopup();
            SetSelected((selection && selection.Active) || (library && library.IsOpen));
        }

        static T Field<T>(object source, string name) where T : class
        {
            return typeof(GameUI).GetField(name, BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(source) as T
                ?? throw new MissingFieldException("Game UI changed: " + name);
        }

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

        void ToggleLibrary()
        {
            if (!library || !WorldFlowManager.Visible) return;
            if (library.IsOpen) { library.ClosePopup(); return; }
            if (selection && selection.Active) selection.Cancel();
            DragDropManager.Instance.CancelDragDrop();
            library.Open();
        }

        internal void StartSelection(string destination)
        {
            if (!selection || !WorldFlowManager.Visible) return;
            try
            {
                captureBookId = destination;
                DragDropManager.Instance.CancelDragDrop();
                GameUI.Instance.CloseAllExcept(null);
                selection.Begin();
                Notify("Drag an area. Hold at a screen edge to pan. Escape cancels.");
            }
            catch (Exception error) { selection.Cancel(); Logger.LogError(error); Notify("Could not start blueprint selection."); }
        }

        internal void SetSelected(bool selected)
        {
            if (activeBorder) activeBorder.SetActive(selected);
        }
        internal void Export(SelectionBox bounds)
        {
            try
            {
                var blueprint = BlueprintCapture.Capture(WorldFlowManager.Instance.CurrentFlow, bounds);
                var entry = library.Save(blueprint, captureBookId);
                Logger.LogInfo("Saved " + entry.FileName + " (" + blueprint.Buildings + " buildings, " + blueprint.Pipes + " internal pipes). Save data was not modified.");
                library.Open(entry.Id, "Blueprint stored. Use a label or type a name, then choose its icon.");
            }
            catch (Exception error)
            {
                Logger.LogWarning("Blueprint export: " + error);
                Notify(error is InvalidOperationException ? error.Message : "Blueprint could not be saved. Check the BepInEx log.");
            }
        }
        void Notify(string message)
        {
            if (GameUI.Instance) GameUI.Instance.ShowWarning(GameInput.MouseUI, message);
        }
        internal void LogLibraryError(Exception error) => Logger.LogWarning("Blueprint library: " + error);
        internal void ReportLibraryError(Exception error)
        {
            LogLibraryError(error);
            Notify("Could not open blueprint library. Your files are preserved; check the BepInEx log.");
        }
        internal void StartPlacement(PortableBlueprint blueprint, string entryId)
        {
            try
            {
                if (!placementHooksReady) throw new InvalidOperationException("The blueprint placement guards are unavailable. Check the BepInEx log.");
                var report = BlueprintResearch.Check(blueprint);
                if (!report.CanPlace) { library.ShowResearch(report); return; }
                library.ClosePopup(); BlueprintPlacement.Begin(this, blueprint, entryId);
            }
            catch (Exception error) { LogLibraryError(error); library.Open(entryId, "Could not prepare this blueprint: " + error.Message); }
        }
        internal void ShowResearchBlocked(string entryId, BlueprintResearchReport report)
        {
            library.Open(entryId, "Research required before workshop placement."); library.ShowResearch(report);
        }
        internal void ShowMessage(string message) => Notify(message);
        void CleanupUi()
        {
            if (library)
            {
                if (library.IsOpen && attachedUi && attachedUi == GameUI.Instance) library.ClosePopup();
                library.gameObject.SetActive(false); Destroy(library.gameObject);
            }
            if (selection) { selection.Cancel(); Destroy(selection.gameObject); }
            if (summary) { summary.Hide(); Destroy(summary.gameObject); }
            if (toolbarButton) Destroy(toolbarButton);
            library = null; summary = null; selection = null; toolbarButton = null; activeBorder = null; attachedUi = null;
        }
        void OnDestroy()
        {
            BlueprintPlacement.CancelOwned(this);
            patches?.UnpatchSelf();
            CleanupUi();
            if (icon) Destroy(icon);
            if (iconTexture) Destroy(iconTexture);
        }
    }
}
