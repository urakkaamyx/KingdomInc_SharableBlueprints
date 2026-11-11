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
        void DrawRectangle()
        {
            var bounds = new SelectionBox(anchor.x, anchor.y, cursor.x, cursor.y);
            rectangle.anchoredPosition = new Vector2(bounds.Left, bounds.Top);
            rectangle.sizeDelta = new Vector2(bounds.Width, bounds.Height);
        }
    }
}
