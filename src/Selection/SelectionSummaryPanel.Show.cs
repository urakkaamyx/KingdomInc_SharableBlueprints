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
        public void Show(SelectionPreview preview, bool dragging)
        {
            gameObject.SetActive(true);
            total.text = preview.Buildings + " buildings · " + preview.Nodes + " nodes\n"
                + preview.Containers + " containers · " + preview.Decorations + " labels/icons";
            string labels = preview.Groups.Count == 0 ? (dragging ? "No complete buildings yet" : "Drag around your buildings")
                : string.Join("\n", preview.Groups.Select(g => g.Name));
            string values = string.Join("\n", preview.Groups.Select(g => "× " + g.Buildings));
            if (names.text != labels) names.text = labels;
            if (counts.text != values) counts.text = values;
            Layout();
        }
    }
}
