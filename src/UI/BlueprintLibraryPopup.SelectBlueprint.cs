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
        void SelectBlueprint(LibraryBlueprint entry)
        {
            selectedId = entry.Id; RefreshBlueprints();
            Run(() => { var report = BlueprintResearch.Check(ReadBlueprint(entry)); if (!report.CanPlace) ShowResearch(report); });
        }
    }
}
