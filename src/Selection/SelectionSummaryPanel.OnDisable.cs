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
        void OnDisable() { base.OnPointerExit(null); }
    }
}
