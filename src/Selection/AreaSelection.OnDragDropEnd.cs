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
        public void OnDragDropEnd()
        {
            bool valid = ContextValid;
            // Switching to another native tool also drops its previous target. Only a
            // real mouse release should export; changing tools cancels this selection.
            if (Mouse.current == null || !Mouse.current.leftButton.wasReleasedThisFrame) { Cancel(); return; }
            if (dragging) RefreshCursor();
            var bounds = new SelectionBox(anchor.x, anchor.y, cursor.x, cursor.y);
            if (!dragging || !bounds.HasArea)
            {
                dragging = false; rectangle.sizeDelta = Vector2.zero;
                anchor = cursor = Vector2Int.zero; nextPreview = 0;
                if (valid) DragDropManager.Instance.SetDragDropTarget(this, false);
                else Cancel();
                return;
            }
            Cancel();
            if (valid) owner.Export(bounds);
        }
    }
}
