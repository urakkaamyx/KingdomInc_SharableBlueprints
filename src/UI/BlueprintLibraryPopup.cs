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
        static readonly Color Ink = new Color(.13f, .105f, .09f), Wood = new Color(.24f, .19f, .15f),
            Gold = new Color(.66f, .51f, .29f), Paper = new Color(.91f, .84f, .66f);
        const float PanelWidth = 1160, PanelHeight = 800;
        BlueprintUiTheme theme;
        SharableBlueprintsPlugin owner;
        TMP_FontAsset font;
        RectTransform panel, bookRows, blueprintRows, picker;
        TMP_InputField search, blueprintName;
        TMP_Text status, details, currentBook;
        Button manageBook, saveName, deleteBlueprint, moveBook, labelChoices, chooseIcon;
        Button copyBlueprint, placeBlueprint, researchDetails;
        Image selectedIcon;
        string editingId, draftIconName;
        BlueprintLibraryStore store;
        string bookFilter = "*", selectedId;
        bool restoreInput;
        int openedFrame;
        public string CaptureBookId => bookFilter == "*" ? "" : bookFilter;
        public bool IsOpen => gameObject.activeSelf;

        public void Initialize(SharableBlueprintsPlugin plugin, GameUI ui, Transform parent)
        {
            owner = plugin;
            theme = new BlueprintUiTheme();
            store = new BlueprintLibraryStore(owner.ExportDirectory);
            font = ui.GetComponentsInChildren<TMP_Text>(true).Select(t => t.font).FirstOrDefault(f => f);
            if (!font) throw new InvalidOperationException("The game's UI font was not found.");
            transform.SetParent(parent, false);
            var root = (RectTransform)transform;
            root.anchorMin = Vector2.zero; root.anchorMax = Vector2.one;
            root.offsetMin = root.offsetMax = Vector2.zero;
            gameObject.AddComponent<Image>().color = new Color(0, 0, 0, .65f);
            var canvas = gameObject.AddComponent<Canvas>(); canvas.overrideSorting = true; canvas.sortingOrder = 100;
            gameObject.AddComponent<GraphicRaycaster>();
            panel = Box(root, "Library", 0, 0, PanelWidth, PanelHeight, Wood);
            panel.anchorMin = panel.anchorMax = panel.pivot = new Vector2(.5f, .5f); panel.anchoredPosition = Vector2.zero;
            IconAt(panel, owner.BlueprintIcon, 30, 28, 56);
            Label(panel, "SHARABLE BLUEPRINTS", 102, 29, 580, 34, 26, Paper);
            Label(panel, "Your workshop plans and blueprint books", 104, 70, 580, 24, 16, Paper);
            var create = ButtonAt(panel, "New Blueprint", 714, 38, 178, 46, NewBlueprint);
            theme.Button(create, true);
            ButtonAt(panel, "Paste Blueprint", 900, 38, 178, 46, ShowPasteDialog);
            ButtonAt(panel, "X", 1090, 38, 42, 46, ClosePopup);

            var books = Box(panel, "Books", 24, 120, 248, 600, Ink);
            SectionTitle(books, "BOOKS", 16, 16, 216);
            bookRows = Scroll(books, "BookList", 10, 60, 228, 404);
            ButtonAt(books, "New Book", 14, 484, 220, 44, () => ShowBookEditor(true));
            manageBook = ButtonAt(books, "Manage Book", 14, 540, 220, 44, () => ShowBookEditor(false));
            manageBook.gameObject.AddComponent<TooltipSource>().SetTooltip("Manage Book", "Select a book to rename or delete it. All Blueprints and Unfiled are permanent.");

            var list = Box(panel, "Blueprints", 288, 120, 358, 600, Ink);
            SectionTitle(list, "BLUEPRINTS", 16, 16, 326);
            search = Input(list, "Search blueprints…", 14, 60, 330, 42);
            search.onValueChanged.AddListener(_ => RefreshBlueprints());
            blueprintRows = Scroll(list, "BlueprintList", 10, 116, 338, 470);

            var inspector = Box(panel, "Details", 662, 120, 474, 600, Ink);
            SectionTitle(inspector, "SELECTED BLUEPRINT", 16, 16, 442);
            Label(inspector, "NAME", 24, 62, 426, 20, 14, Gold);
            blueprintName = Input(inspector, "Select a blueprint", 20, 88, 434, 42);
            labelChoices = ButtonAt(inspector, "Names from labels", 20, 142, 434, 38, ShowLabelDropdown);
            var iconFrame = Box(inspector, "IconSlot", 20, 194, 58, 58, Ink);
            selectedIcon = IconAt(iconFrame, owner.BlueprintIcon, 9, 9, 40);
            chooseIcon = ButtonAt(inspector, "Choose Icon", 90, 200, 364, 46, ShowIconPicker);
            saveName = ButtonAt(inspector, "Save Name & Icon", 20, 268, 304, 42, () => Run(() => {
                var entry = SelectedEntry();
                if (entry == null) return;
                BlueprintIdentity.SaveMetadata(store, entry, blueprintName.text, draftIconName);
                editingId = null; search.SetTextWithoutNotify(""); Refresh(); SetStatus("Blueprint name and icon saved.");
            }));
            deleteBlueprint = ButtonAt(inspector, "Delete", 336, 268, 118, 42, ConfirmDeleteBlueprint);
            theme.Button(deleteBlueprint, false, true);
            details = Label(inspector, "", 24, 328, 426, 76, 16, Paper);
            details.textWrappingMode = TextWrappingModes.Normal;
            currentBook = Label(inspector, "", 24, 414, 426, 24, 16, Paper);
            moveBook = ButtonAt(inspector, "Move to Book", 20, 448, 434, 40, ShowBookPicker);
            researchDetails = ButtonAt(inspector, "Research", 20, 500, 434, 32, () => Run(() => {
                var entry = SelectedEntry(); if (entry != null) ShowResearch(BlueprintResearch.Check(ReadBlueprint(entry)));
            }));
            copyBlueprint = ButtonAt(inspector, "Copy Share Code", 20, 546, 208, 40, CopyToClipboard);
            placeBlueprint = ButtonAt(inspector, "Place in Workshop", 240, 546, 214, 40, () => Run(() => {
                var entry = SelectedEntry(); if (entry != null) owner.StartPlacement(ReadBlueprint(entry), entry.Id);
            }));
            theme.Button(placeBlueprint, true);
            status = Label(panel, "", 32, 742, 1096, 34, 15, Paper);
            status.textWrappingMode = TextWrappingModes.Normal;
            gameObject.SetActive(false);
        }
    }
}
