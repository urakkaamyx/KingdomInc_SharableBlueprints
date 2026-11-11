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
        void LateUpdate()
        {
            if (!ContextValid) { Cancel(); return; }
            if (dragging)
            {
                PanAtScreenEdge();
                // Reproject after the camera has moved. The anchor stays in world space.
                RefreshCursor(); DrawRectangle();
            }
            if (Time.unscaledTime >= nextPreview)
            {
                nextPreview = Time.unscaledTime + .1f;
                summary.Show(SelectionContents.Preview(flow, new SelectionBox(anchor.x, anchor.y, cursor.x, cursor.y)), dragging);
            }
        }
    }
}
