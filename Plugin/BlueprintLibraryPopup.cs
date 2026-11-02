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
    public sealed class BlueprintLibraryPopup : UIPopupMenu
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

        public void Open(string select = null, string message = null)
        {
            try
            {
                store.Load();
                if (bookFilter != "*" && bookFilter != "" && !store.Books.Any(b => b.Id == bookFilter)) bookFilter = "*";
                if (select != null)
                {
                    editingId = null;
                    selectedId = select;
                    var entry = store.Blueprints.FirstOrDefault(b => b.Id == select);
                    if (entry != null) bookFilter = entry.BookId;
                    search.SetTextWithoutNotify("");
                }
                Refresh();
                SetStatus(message ?? "Choose a book or select an area with New Blueprint.");
                GameUI.Instance.TogglePopupMenu(this, true);
                transform.SetAsLastSibling();
                openedFrame = Time.frameCount;
                restoreInput = GameInput.Enabled;
                GameInput.Disable();
            }
            catch (Exception error) { owner.ReportLibraryError(error); }
        }
        void Update()
        {
            // Native menus suspend game shortcuts while typing; keep mouse UI working.
            if (Time.frameCount == openedFrame) return;
            if ((Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                || (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame))
            {
                if (picker) DismissPicker();
                else ClosePopup();
            }
            var parent = transform.parent as RectTransform;
            if (parent)
            {
                float scale = Mathf.Min(1, (parent.rect.width - 32) / PanelWidth, (parent.rect.height - 32) / PanelHeight);
                panel.localScale = Vector3.one * Mathf.Max(.1f, scale);
            }
        }
        public override void PopupClosed()
        {
            if (EventSystem.current && EventSystem.current.currentSelectedGameObject
                && EventSystem.current.currentSelectedGameObject.transform.IsChildOf(transform))
                EventSystem.current.SetSelectedGameObject(null);
            if (picker) DismissPicker();
            owner.SetSelected(false);
            base.PopupClosed();
        }
        void OnDisable()
        {
            // Also release the native zoom blocker when deactivated outside TogglePopupMenu.
            base.OnPointerExit(null);
            if (restoreInput && GameUI.Instance && !GameUI.MenuVisible) GameInput.Enable();
            restoreInput = false;
        }
        void NewBlueprint()
        {
            string destination = CaptureBookId;
            ClosePopup(); owner.StartSelection(destination);
        }
        void Run(Action action)
        {
            try { action(); }
            catch (Exception error) { SetStatus(error.Message); owner.LogLibraryError(error); }
        }
        public LibraryBlueprint Save(CapturedBlueprint capture, string destination)
        {
            store.Load();
            if (!string.IsNullOrEmpty(destination) && !store.Books.Any(b => b.Id == destination)) destination = "";
            string name = capture.Suggestions.DefaultName("Blueprint " + (store.Blueprints.Count + 1));
            capture.Document["name"] = name; capture.Document["icon"] = capture.Suggestions.DefaultIcon;
            BlueprintIdentity.WriteSuggestions(capture.Document, capture.Suggestions);
            return store.Add(capture.Document.ToString(true), name, destination, capture.Buildings, capture.Pipes, capture.Decorations, capture.Suggestions);
        }
        void Refresh()
        {
            Clear(bookRows);
            BookRow("All Blueprints", "*", store.Blueprints.Count);
            BookRow("Unfiled", "", store.Blueprints.Count(b => b.BookId == ""));
            foreach (var book in store.Books.OrderBy(b => b.Name, StringComparer.OrdinalIgnoreCase))
                BookRow(book.Name, book.Id, store.Blueprints.Count(b => b.BookId == book.Id));
            var selectedBook = store.Books.FirstOrDefault(b => b.Id == bookFilter);
            manageBook.interactable = selectedBook != null;
            RefreshBlueprints();
        }
        void BookRow(string name, string id, int count)
        {
            var button = Row(bookRows, name, () => { bookFilter = id; Refresh(); }, bookFilter == id);
            var title = button.GetComponentInChildren<TMP_Text>();
            title.rectTransform.anchoredPosition = new Vector2(18, 0);
            title.rectTransform.sizeDelta = new Vector2(((RectTransform)button.transform).sizeDelta.x - 60, 54);
            var total = Label(button.transform, count.ToString(), ((RectTransform)button.transform).sizeDelta.x - 40, 0, 24, 54, 15, Paper);
            total.enableAutoSizing = true; total.fontSizeMin = 12; total.fontSizeMax = 15;
            total.alignment = TextAlignmentOptions.MidlineRight;
        }
        void RefreshBlueprints()
        {
            Clear(blueprintRows);
            var matches = store.Blueprints.Where(b => (bookFilter == "*" || b.BookId == bookFilter)
                && b.Name.IndexOf(search.text ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                .OrderBy(b => b.Name, StringComparer.OrdinalIgnoreCase).ToList();
            foreach (var entry in matches)
            {
                var row = Row(blueprintRows, entry.Name + (File.Exists(store.FilePath(entry)) ? "" : " [file missing]"),
                    () => SelectBlueprint(entry), selectedId == entry.Id);
                var title = row.GetComponentInChildren<TMP_Text>();
                title.rectTransform.anchoredPosition = new Vector2(62, 0);
                title.rectTransform.sizeDelta = new Vector2(((RectTransform)row.transform).sizeDelta.x - 82, 54);
                IconAt(row.transform, BlueprintIdentity.Sprite(entry.IconName) ?? owner.BlueprintIcon, 20, 11, 32);
            }
            if (matches.Count == 0) Row(blueprintRows, "No blueprints here yet", null, false);
            if (!matches.Any(b => b.Id == selectedId)) selectedId = null;
            ShowDetails();
        }
        void ShowDetails()
        {
            var entry = SelectedEntry();
            if (entry == null || editingId != entry.Id)
            {
                if (entry != null)
                    try { BlueprintIdentity.Hydrate(entry, store.FilePath(entry)); }
                    catch (Exception error) { owner.LogLibraryError(error); }
                editingId = entry?.Id;
                blueprintName.SetTextWithoutNotify(entry?.Name ?? "");
                draftIconName = entry?.IconName ?? "";
            }
            selectedIcon.sprite = BlueprintIdentity.Sprite(draftIconName) ?? owner.BlueprintIcon;
            saveName.interactable = moveBook.interactable = blueprintName.interactable = chooseIcon.interactable = entry != null;
            deleteBlueprint.interactable = entry != null;
            copyBlueprint.interactable = placeBlueprint.interactable = researchDetails.interactable = entry != null;
            if (entry != null)
            {
                try
                {
                    var report = BlueprintResearch.Check(ReadBlueprint(entry));
                    researchDetails.GetComponentInChildren<TMP_Text>().text = report.CanPlace ? "Research: Ready" : "Research Required (" + report.Issues.Count + ")";
                    theme.Button(researchDetails, false, !report.CanPlace);
                }
                catch { researchDetails.GetComponentInChildren<TMP_Text>().text = "Blueprint file unavailable"; }
            }
            else researchDetails.GetComponentInChildren<TMP_Text>().text = "Research";
            labelChoices.interactable = entry != null && entry.Suggestions.Labels.Count > 0;
            labelChoices.GetComponentInChildren<TMP_Text>().text = entry != null && entry.Suggestions.Labels.Count > 0
                ? "Names from Labels (" + entry.Suggestions.Labels.Count + ")  v" : "No labels in this blueprint";
            details.text = entry == null ? "Select a blueprint to view its details.\nUse New Blueprint to capture an area."
                : (entry.Buildings == 0 ? "Earlier blueprint export" : entry.Buildings + " buildings · " + entry.Pipes + " connections\n" + entry.Decorations + " decorations")
                    + "\nSaved " + entry.CreatedUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm");
            currentBook.text = entry == null ? "" : "Book: " + (store.Books.FirstOrDefault(b => b.Id == entry.BookId)?.Name ?? "Unfiled");
        }
        LibraryBlueprint SelectedEntry() => store.Blueprints.FirstOrDefault(b => b.Id == selectedId);
        void ShowBookEditor(bool create)
        {
            if (picker) return;
            var book = store.Books.FirstOrDefault(b => b.Id == bookFilter);
            if (!create && book == null) return;
            var overlay = PickerOverlay("BookEditor");
            var box = Dialog(overlay, "Book", 540, create ? 296 : 356);
            box.gameObject.AddComponent<Button>();
            Label(box, create ? "NEW BLUEPRINT BOOK" : "MANAGE BLUEPRINT BOOK", 24, 22, 492, 30, 21, Paper);
            Label(box, create ? "Give your collection a name." : "Rename this book or remove it from your library.", 24, 66, 492, 36, 16, Paper);
            var name = Input(box, "Book name", 24, 116, 492, 44);
            name.SetTextWithoutNotify(create ? "" : book.Name);
            var errorLabel = Label(box, "", 24, 174, 492, 42, 14, Paper);
            var save = ButtonAt(box, create ? "Create Book" : "Save Name", 24, 230, 236, 42, () => {
                try
                {
                    if (create) bookFilter = store.CreateBook(name.text).Id;
                    else store.RenameBook(book.Id, name.text);
                    DismissPicker(); Refresh();
                    SetStatus(create ? "Book created. New blueprints will be stored here." : "Book renamed.");
                }
                catch (Exception error) { errorLabel.text = error.Message; owner.LogLibraryError(error); }
            });
            theme.Button(save, true);
            ButtonAt(box, "Cancel", 280, 230, 236, 42, DismissPicker);
            if (!create)
            {
                var remove = ButtonAt(box, "Delete Book", 24, 294, 492, 42, () => { DismissPicker(); ConfirmDeleteBook(); });
                theme.Button(remove, false, true);
            }
        }
        void ConfirmDeleteBlueprint()
        {
            var entry = SelectedEntry(); if (entry == null || picker) return;
            ConfirmDelete("DELETE BLUEPRINT", entry.Name,
                "Remove this blueprint from the library?\nA recoverable copy will be kept locally.", () => {
                    store.DeleteBlueprint(entry.Id); selectedId = editingId = null;
                }, "Blueprint deleted. A recoverable copy is kept in the library's _Deleted folder.");
        }
        void ConfirmDeleteBook()
        {
            var book = store.Books.FirstOrDefault(b => b.Id == bookFilter); if (book == null || picker) return;
            int count = store.Blueprints.Count(b => b.BookId == book.Id);
            ConfirmDelete("DELETE BLUEPRINT BOOK", book.Name,
                "Remove this book? Its " + count + " blueprint(s) will be kept in Unfiled.", () => {
                    store.DeleteBook(book.Id); bookFilter = "";
                }, "Book deleted. Its blueprints are kept in Unfiled.");
        }
        void ConfirmDelete(string title, string name, string message, Action action, string success)
        {
            var overlay = PickerOverlay("ConfirmDelete");
            var box = Dialog(overlay, "Confirm", 540, 308); box.gameObject.AddComponent<Button>();
            Label(box, title, 18, 14, 504, 28, 21, Paper);
            Label(box, name, 18, 54, 504, 66, 21, Paper);
            Label(box, message, 18, 128, 504, 66, 16, Paper);
            var errorLabel = Label(box, "", 18, 198, 504, 40, 14, Paper);
            var confirm = ButtonAt(box, "Delete", 18, 256, 242, 34, () => {
                try { action(); DismissPicker(); Refresh(); SetStatus(success); }
                catch (Exception error) { errorLabel.text = error.Message; owner.LogLibraryError(error); }
            });
            theme.Button(confirm, false, true);
            ButtonAt(box, "Cancel", 280, 256, 242, 34, DismissPicker);
        }
        PortableBlueprint ReadBlueprint(LibraryBlueprint entry) => PortableBlueprint.Parse(File.ReadAllText(store.FilePath(entry)));
        void SelectBlueprint(LibraryBlueprint entry)
        {
            selectedId = entry.Id; RefreshBlueprints();
            Run(() => { var report = BlueprintResearch.Check(ReadBlueprint(entry)); if (!report.CanPlace) ShowResearch(report); });
        }
        void CopyToClipboard()
        {
            Run(() => {
                var entry = SelectedEntry(); if (entry == null) return;
                var blueprint = ReadBlueprint(entry);
                blueprint.Document["name"] = entry.Name; blueprint.Document["icon"] = entry.IconName;
                BlueprintIdentity.WriteSuggestions(blueprint.Document, entry.Suggestions);
                string code = BlueprintShareCode.EncodeCompact(blueprint.Document.ToString(false));
                var parts = BlueprintShareCode.ForDiscord(code);
                if (parts.Count == 1)
                {
                    GUIUtility.systemCopyBuffer = "```\n" + code + "\n```";
                    SetStatus("Compact code copied: " + code.Length + " characters — fits one Discord message.");
                }
                else ShowDiscordParts(code, parts);
            });
        }
        void ShowDiscordParts(string code, System.Collections.Generic.List<string> parts)
        {
            DismissPicker(); var overlay = PickerOverlay("DiscordShare");
            var box = Dialog(overlay, "Share", 668, 472); box.gameObject.AddComponent<Button>();
            Label(box, "SHARE BLUEPRINT IN DISCORD", 18, 14, 632, 30, 22, Paper);
            Label(box, code.Length + " characters · " + parts.Count + " messages\nCopy and send each part. The recipient pastes all parts together.", 18, 58, 632, 62, 16, Paper);
            var rows = Scroll(box, "Parts", 16, 132, 636, 258);
            for (int i = 0; i < parts.Count; i++)
            {
                int index = i;
                Row(rows, "Copy Part " + (i + 1) + " / " + parts.Count + "  (" + parts[i].Length + " characters)", () => {
                    GUIUtility.systemCopyBuffer = "```\n" + parts[index] + "\n```"; SetStatus("Discord part " + (index + 1) + " copied.");
                }, false);
            }
            ButtonAt(box, "Copy Whole Code", 18, 420, 300, 32, () => { GUIUtility.systemCopyBuffer = "```\n" + code + "\n```"; SetStatus("Whole compact blueprint code copied."); });
            ButtonAt(box, "Done", 350, 420, 300, 32, DismissPicker);
        }
        void ShowPasteDialog()
        {
            if (picker) return;
            var overlay = PickerOverlay("PasteBlueprint");
            var box = Dialog(overlay, "Paste", 668, 472); box.gameObject.AddComponent<Button>();
            Label(box, "PASTE INTO BLUEPRINT LIBRARY", 18, 14, 632, 30, 22, Paper);
            Label(box, "Locked buildings can be stored. Research is checked when placing.", 18, 54, 632, 42, 15, Paper);
            var text = Input(box, "Paste a compact code, all Discord parts, or blueprint JSON here…", 18, 106, 632, 268);
            text.characterLimit = 0; text.lineType = TMP_InputField.LineType.MultiLineNewline;
            text.textComponent.textWrappingMode = TextWrappingModes.Normal; text.textComponent.alignment = TextAlignmentOptions.TopLeft;
            text.textComponent.fontSize = 13;
            text.SetTextWithoutNotify(GUIUtility.systemCopyBuffer ?? "");
            var errorLabel = Label(box, "", 18, 380, 632, 36, 14, Paper);
            ButtonAt(box, "Store Blueprint", 18, 426, 300, 30, () => {
                try
                {
                    var blueprint = PortableBlueprint.Parse(text.text);
                    var suggestions = BlueprintIdentity.FromDocument(blueprint.Document);
                    blueprint.Document["name"] = blueprint.Name;
                    var entry = store.Add(blueprint.Document.ToString(true), blueprint.Name, CaptureBookId,
                        blueprint.Buildings, blueprint.Pipes, blueprint.Decorations, suggestions, blueprint.Icon);
                    DismissPicker(); Open(entry.Id, "Blueprint stored. Locked content stays in your library until you research it.");
                    Run(() => { var report = BlueprintResearch.Check(blueprint); if (!report.CanPlace) ShowResearch(report); });
                }
                catch (Exception error) { errorLabel.text = error.Message; owner.LogLibraryError(error); }
            });
            ButtonAt(box, "Cancel", 350, 426, 300, 30, DismissPicker);
        }
        public void ShowResearch(BlueprintResearchReport report)
        {
            DismissPicker();
            var overlay = PickerOverlay("BlueprintResearch");
            var box = Dialog(overlay, "Research", 668, 506); box.gameObject.AddComponent<Button>();
            Label(box, report.CanPlace ? "RESEARCH READY" : "CANNOT PLACE — RESEARCH REQUIRED", 18, 14, 632, 32, 21, Paper);
            Label(box, "You can still paste, store, organize and share this blueprint.", 18, 54, 632, 30, 15, Paper);
            var rows = Scroll(box, "Requirements", 16, 98, 636, 344);
            if (report.CanPlace) Row(rows, "All required research is available in this kingdom.", null, false);
            foreach (var issue in report.Issues)
            {
                string message = issue.Name + "\n" + issue.Description + "\nNeeded by: " + string.Join(", ", issue.Affected.OrderBy(s => s));
                float width = rows.sizeDelta.x - 6;
                var label = Label(rows, message, 8, 0, width, 100, 16, Paper);
                label.margin = new Vector4(8, 8, 8, 8); label.textWrappingMode = TextWrappingModes.Normal;
                label.gameObject.AddComponent<LayoutElement>().preferredHeight = Mathf.Max(96, label.GetPreferredValues(message, width, float.PositiveInfinity).y + 24);
            }
            ButtonAt(box, "Back to Library", 18, 460, 632, 30, DismissPicker);
        }
        void DismissPicker()
        {
            if (picker) { picker.gameObject.SetActive(false); Destroy(picker.gameObject); picker = null; }
        }
        RectTransform PickerOverlay(string name)
        {
            picker = Rect(panel, name, 0, 0, PanelWidth, PanelHeight);
            var shade = picker.gameObject.AddComponent<Image>(); shade.color = new Color(0, 0, 0, .45f);
            var dismiss = picker.gameObject.AddComponent<Button>(); dismiss.targetGraphic = shade; dismiss.onClick.AddListener(DismissPicker);
            return picker;
        }
        void ShowLabelDropdown()
        {
            var entry = SelectedEntry();
            if (entry == null || entry.Suggestions.Labels.Count == 0 || picker) return;
            var overlay = PickerOverlay("LabelDropdown");
            float height = Mathf.Min(300, entry.Suggestions.Labels.Count * 54 + 12);
            var box = Box(overlay, "Labels", 682, 302, 434, height, Wood);
            box.gameObject.AddComponent<Button>(); // Swallow clicks in the dropdown's padding.
            var rows = Scroll(box, "LabelChoices", 8, 8, 418, height - 16);
            foreach (string name in entry.Suggestions.Labels)
                Row(rows, name, () => { blueprintName.SetTextWithoutNotify(name); DismissPicker(); }, blueprintName.text == name);
        }
        void ShowIconPicker()
        {
            var entry = SelectedEntry();
            if (entry == null || picker) return;
            var overlay = PickerOverlay("IconPicker");
            var box = Dialog(overlay, "Icons", 640, 472); box.gameObject.AddComponent<Button>();
            Label(box, "CHOOSE BLUEPRINT ICON", 18, 14, 600, 30, 22, Paper);
            var fromSelection = ButtonAt(box, "From Selection (" + entry.Suggestions.Icons.Count + ")", 16, 56, 300, 32, null);
            var allIcons = ButtonAt(box, "All Game Icons", 324, 56, 300, 32, null);
            var filter = Input(box, "Search icons…", 16, 100, 608, 32);
            var grid = IconGrid(box, 16, 144, 608, 264);
            var empty = Label(box, "", 24, 154, 590, 42, 16, Paper);
            bool selectionOnly = entry.Suggestions.Icons.Count > 0;
            var catalog = BlueprintIdentity.Catalog();
            Action redraw = () => {
                Clear(grid);
                var options = selectionOnly ? entry.Suggestions.Icons : catalog;
                var available = options.Where(i => (i.Name ?? i.Id).IndexOf(filter.text ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    || i.Id.IndexOf(filter.text ?? "", StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                empty.text = available.Count == 0 ? "No matching icons." : "";
                theme.Button(fromSelection, selectionOnly);
                theme.Button(allIcons, !selectionOnly);
                foreach (var option in available)
                {
                    Sprite sprite = BlueprintIdentity.Sprite(option.Id);
                    if (!sprite) continue;
                    var button = ButtonAt(grid, "", 0, 0, 66, 66, () => {
                        draftIconName = option.Id; selectedIcon.sprite = sprite; DismissPicker();
                        SetStatus("Icon selected. Save Name & Icon to keep your changes.");
                    });
                    IconAt(button.transform, sprite, 13, 13, 40);
                    theme.Button(button, draftIconName == option.Id);
                    button.gameObject.AddComponent<TooltipSource>().SetTooltip(option.Name, "Use this blueprint icon.");
                }
            };
            fromSelection.interactable = allIcons.interactable = true;
            fromSelection.onClick.AddListener(() => { selectionOnly = true; redraw(); });
            allIcons.onClick.AddListener(() => { selectionOnly = false; redraw(); });
            filter.onValueChanged.AddListener(_ => redraw());
            ButtonAt(box, "Use Schematic", 16, 424, 220, 32, () => {
                draftIconName = ""; selectedIcon.sprite = owner.BlueprintIcon; DismissPicker();
                SetStatus("Schematic icon selected. Save Name & Icon to keep it.");
            });
            ButtonAt(box, "Cancel", 404, 424, 220, 32, DismissPicker);
            redraw();
        }
        void ShowBookPicker()
        {
            if (selectedId == null || picker) return;
            var overlay = PickerOverlay("BookPicker");
            var box = Dialog(overlay, "ChooseBook", 452, 456); box.gameObject.AddComponent<Button>();
            Label(box, "MOVE BLUEPRINT TO BOOK", 18, 16, 416, 30, 20, Paper);
            var rows = Scroll(box, "Destinations", 14, 60, 424, 328);
            Action<string> move = id => Run(() => {
                store.MoveBlueprint(selectedId, id); bookFilter = id;
                DismissPicker(); Refresh(); SetStatus("Blueprint moved.");
            });
            Row(rows, "Unfiled", () => move(""), false);
            foreach (var book in store.Books.OrderBy(b => b.Name, StringComparer.OrdinalIgnoreCase))
                Row(rows, book.Name, () => move(book.Id), false);
            ButtonAt(box, "Cancel", 14, 404, 424, 36, DismissPicker);
        }
        void SetStatus(string message) { status.text = message; }
        static void Clear(Transform parent)
        {
            foreach (Transform child in parent) { child.gameObject.SetActive(false); Destroy(child.gameObject); }
        }
        static RectTransform Rect(Transform parent, string name, float x, float y, float w, float h)
        {
            var rect = (RectTransform)new GameObject(name, typeof(RectTransform)).transform;
            rect.SetParent(parent, false); rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(x, -y); rect.sizeDelta = new Vector2(w, h); return rect;
        }
        RectTransform Box(Transform parent, string name, float x, float y, float w, float h, Color color)
        {
            var rect = Rect(parent, name, x, y, w, h);
            theme.Image(rect.gameObject.AddComponent<Image>(), color == Ink || name == "Input" ? "ItemSlot_0" : "Tooltip", color);
            return rect;
        }
        RectTransform Dialog(Transform parent, string name, float w, float h)
        {
            var box = Box(parent, name, 0, 0, w, h, Wood);
            box.anchorMin = box.anchorMax = box.pivot = new Vector2(.5f, .5f);
            box.anchoredPosition = Vector2.zero;
            return box;
        }
        void SectionTitle(Transform parent, string text, float x, float y, float w)
        {
            var bar = Rect(parent, "SectionTitle", x, y, w, 32);
            theme.Image(bar.gameObject.AddComponent<Image>(), "TitleBlue", Wood);
            var label = Label(bar, text, 12, 0, w - 24, 32, 16, Paper);
            label.alignment = TextAlignmentOptions.MidlineLeft;
        }
        TMP_Text Label(Transform parent, string text, float x, float y, float w, float h, float size, Color color)
        {
            var label = Rect(parent, "Label", x, y, w, h).gameObject.AddComponent<TextMeshProUGUI>();
            label.font = font; label.fontSize = size; label.text = text; label.color = color;
            label.richText = false; label.raycastTarget = false; label.overflowMode = TextOverflowModes.Ellipsis;
            label.textWrappingMode = TextWrappingModes.Normal;
            return label;
        }
        Button ButtonAt(Transform parent, string text, float x, float y, float w, float h, Action action)
        {
            var rect = Rect(parent, "Button", x, y, w, h);
            rect.gameObject.AddComponent<Image>();
            var button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = rect.GetComponent<Image>();
            theme.Button(button);
            float inset = w < 60 ? 8 : w < 120 ? 12 : 18;
            var label = Label(rect, text, inset, 0, w - inset * 2, h, 16, Paper);
            label.alignment = TextAlignmentOptions.Midline;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.enableAutoSizing = true; label.fontSizeMin = 14; label.fontSizeMax = 16;
            if (action != null) button.onClick.AddListener(() => action()); else button.interactable = false;
            return button;
        }
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
        RectTransform Scroll(Transform parent, string name, float x, float y, float w, float h)
        {
            var rect = Rect(parent, name, x, y, w, h);
            var scroll = rect.gameObject.AddComponent<ScrollRect>();
            var viewport = Rect(rect, "Viewport", 0, 0, w - 24, h); viewport.gameObject.AddComponent<RectMask2D>();
            var hit = viewport.gameObject.AddComponent<Image>(); hit.color = new Color(0, 0, 0, .01f);
            var content = Rect(viewport, "Content", 0, 0, w - 24, 0);
            var layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(3, 3, 3, 3); layout.spacing = 8;
            layout.childControlWidth = layout.childControlHeight = true;
            layout.childForceExpandWidth = true; layout.childForceExpandHeight = false;
            content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.viewport = viewport; scroll.content = content; scroll.horizontal = false; scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped; scroll.scrollSensitivity = 30;
            AddScrollbar(rect, scroll, w, h);
            return content;
        }
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
        static Image IconAt(Transform parent, Sprite sprite, float x, float y, float size)
        {
            var image = Rect(parent, "Icon", x, y, size, size).gameObject.AddComponent<Image>();
            image.sprite = sprite; image.preserveAspect = true; image.raycastTarget = false; return image;
        }
        RectTransform IconGrid(Transform parent, float x, float y, float width, float height)
        {
            var rect = Rect(parent, "IconScroll", x, y, width, height);
            var scroll = rect.gameObject.AddComponent<ScrollRect>();
            var viewport = Rect(rect, "Viewport", 0, 0, width - 24, height); viewport.gameObject.AddComponent<RectMask2D>();
            viewport.gameObject.AddComponent<Image>().color = new Color(0, 0, 0, .1f);
            var content = Rect(viewport, "Grid", 0, 0, width - 24, 0);
            var grid = content.gameObject.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(66, 66); grid.spacing = new Vector2(8, 8); grid.padding = new RectOffset(8, 8, 8, 8);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = Mathf.Max(1, Mathf.FloorToInt((width - 32) / 74));
            content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.viewport = viewport; scroll.content = content; scroll.horizontal = false; scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped; scroll.scrollSensitivity = 36;
            AddScrollbar(rect, scroll, width, height);
            return content;
        }
        Button Row(Transform parent, string text, Action action, bool selected)
        {
            var button = ButtonAt(parent, text, 0, 0, ((RectTransform)parent).sizeDelta.x - 6, 54, action);
            button.gameObject.AddComponent<LayoutElement>().preferredHeight = 54;
            button.GetComponentInChildren<TMP_Text>().alignment = TextAlignmentOptions.MidlineLeft;
            theme.Button(button, selected);
            if (!string.IsNullOrEmpty(text)) button.gameObject.AddComponent<TooltipSource>().SetTooltip(text, "");
            return button;
        }
    }
}
