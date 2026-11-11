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
        public Vector2Int GetGrabOffset(Vector2Int mousePosition) => Vector2Int.zero;
    }
}
