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
        public void Cancel()
        {
            if (DragDropManager.Instance && ReferenceEquals(DragDropManager.Instance.Target, this)) DragDropManager.Instance.ClearDragDrop();
            Active = false; dragging = false; player = null; flow = null;
            if (owner) owner.SetSelected(false);
            if (summary) summary.Hide();
            gameObject.SetActive(false);
        }
    }
}
