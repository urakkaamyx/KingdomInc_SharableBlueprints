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
    }
}
