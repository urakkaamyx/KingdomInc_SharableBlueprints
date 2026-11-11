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
        void Edge(string name, Vector2 min, Vector2 max, Vector2 size)
        {
            var edge = new GameObject(name, typeof(RectTransform), typeof(Image));
            edge.transform.SetParent(transform, false);
            var rect = (RectTransform)edge.transform;
            rect.anchorMin = min; rect.anchorMax = max; rect.sizeDelta = size; rect.anchoredPosition = Vector2.zero;
            var image = edge.GetComponent<Image>(); image.raycastTarget = false;
            image.color = new Color(.85f, .72f, .43f, 1);
        }
    }
}
