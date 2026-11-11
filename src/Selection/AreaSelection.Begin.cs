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
        public void Begin()
        {
            player = GamePlayer.Current; flow = player.ActiveFlow;
            dragging = false; Active = true;
            anchor = cursor = Vector2Int.zero; nextPreview = 0;
            rectangle.sizeDelta = Vector2.zero;
            gameObject.SetActive(true); transform.SetAsLastSibling();
            owner.SetSelected(true);
            summary.Show(SelectionPreview.Count(System.Array.Empty<SelectionBuilding>(), new SelectionBox()), false);
            if (!DragDropManager.Instance.SetDragDropTarget(this, false)) Cancel();
        }
    }
}
