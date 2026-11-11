using Assets.Behaviour.Flow;
using Assets.Behaviour.UI;
using Assets.Source.Flow;
using Assets.Source.Player;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using GameInput = Assets.Behaviour.Input.PlayerInput;

namespace SharableBlueprints
{
    public sealed partial class AreaSelection : MonoBehaviour, IDragDropTarget
    {
        void Update()
        {
            if (!ContextValid) { Cancel(); return; }
            if (!dragging && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                // Native UI controls remain usable without beginning a world selection.
                foreach (var ui in UIHelper.GetAllMouseOverUI())
                    if (!ui.GetComponentInParent<WorldFlowElement>() && !ui.GetComponentInParent<WorldFlowDeco>() && !ui.GetComponentInParent<WorldFlowPipe>()) return;
                RefreshCursor(); anchor = cursor; dragging = true;
                nextPreview = 0;
            }
        }
    }
}
