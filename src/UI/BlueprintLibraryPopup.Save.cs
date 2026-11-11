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
        public LibraryBlueprint Save(CapturedBlueprint capture, string destination)
        {
            store.Load();
            if (!string.IsNullOrEmpty(destination) && !store.Books.Any(b => b.Id == destination)) destination = "";
            string name = capture.Suggestions.DefaultName("Blueprint " + (store.Blueprints.Count + 1));
            capture.Document["name"] = name; capture.Document["icon"] = capture.Suggestions.DefaultIcon;
            BlueprintIdentity.WriteSuggestions(capture.Document, capture.Suggestions);
            return store.Add(capture.Document.ToString(true), name, destination, capture.Buildings, capture.Pipes, capture.Decorations, capture.Suggestions);
        }
    }
}
