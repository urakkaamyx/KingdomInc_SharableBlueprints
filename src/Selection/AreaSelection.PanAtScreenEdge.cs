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
        void PanAtScreenEdge()
        {
            var mouse = Mouse.current;
            if (!Application.isFocused || mouse == null || !mouse.leftButton.isPressed || !WorldFlowCamera.Instance || !Camera.main) return;
            Vector2 position = mouse.position.ReadValue();
            if (position.x < 0 || position.y < 0 || position.x > Screen.width || position.y > Screen.height) return;
            float margin = Mathf.Clamp(Screen.height * .04f, 28, 72);
            var direction = new Vector2((float)SelectionEdgePan.Axis(position.x, Screen.width, margin),
                (float)SelectionEdgePan.Axis(position.y, Screen.height, margin));
            if (direction.sqrMagnitude == 0) return;
            if (direction.sqrMagnitude > 1) direction.Normalize();
            float unitsPerPixel = 2 * Camera.main.orthographicSize / Mathf.Max(1, Screen.height);
            Vector2 movement = direction * (600 * unitsPerPixel * Mathf.Min(Time.unscaledDeltaTime, .05f));
            WorldFlowCamera.Instance.Move(movement.x, movement.y);
        }
    }
}
