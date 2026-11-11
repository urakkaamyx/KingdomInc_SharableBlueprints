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
        SharableBlueprintsPlugin owner;
        GamePlayer player;
        FlowDiagram flow;
        RectTransform rectangle;
        Vector2Int anchor, cursor;
        bool dragging;
        SelectionSummaryPanel summary;
        float nextPreview;
        static readonly GameUIType[] Menus = (GameUIType[])System.Enum.GetValues(typeof(GameUIType));
        public bool Active { get; private set; }
        public bool ContextValid
        {
            get
            {
                if (!Active || GamePlayer.Current != player || player == null || player.ActiveFlow != flow
                    || !WorldFlowManager.Visible || !GameUI.Instance || GameUI.MenuVisible) return false;
                foreach (var menu in Menus) if (GameUI.Instance.IsActive(menu)) return false;
                return true;
            }
        }

        public void Initialize(SharableBlueprintsPlugin plugin, SelectionSummaryPanel summaryPanel, RectTransform nativeArea)
        {
            owner = plugin;
            summary = summaryPanel;
            rectangle = (RectTransform)transform;
            // MouseWorld uses the native overlay parent's world origin. A top-left
            // anchor adds the entire parent height to the drawn world position.
            rectangle.anchorMin = nativeArea.anchorMin;
            rectangle.anchorMax = nativeArea.anchorMax;
            rectangle.pivot = nativeArea.pivot;
            rectangle.localScale = nativeArea.localScale;
            rectangle.localRotation = nativeArea.localRotation;
            rectangle.anchoredPosition3D = Vector3.zero;
            var fill = gameObject.AddComponent<Image>(); fill.raycastTarget = false;
            fill.color = new Color(.77f, .65f, .38f, .12f);
            Edge("Top", new Vector2(0, 1), Vector2.one, new Vector2(0, 3));
            Edge("Bottom", Vector2.zero, new Vector2(1, 0), new Vector2(0, 3));
            Edge("Left", Vector2.zero, new Vector2(0, 1), new Vector2(3, 0));
            Edge("Right", new Vector2(1, 0), Vector2.one, new Vector2(3, 0));
            gameObject.SetActive(false);
        }
    }
}
