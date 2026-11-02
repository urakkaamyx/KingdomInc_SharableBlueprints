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
    public sealed class AreaSelection : MonoBehaviour, IDragDropTarget
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
        void Edge(string name, Vector2 min, Vector2 max, Vector2 size)
        {
            var edge = new GameObject(name, typeof(RectTransform), typeof(Image));
            edge.transform.SetParent(transform, false);
            var rect = (RectTransform)edge.transform;
            rect.anchorMin = min; rect.anchorMax = max; rect.sizeDelta = size; rect.anchoredPosition = Vector2.zero;
            var image = edge.GetComponent<Image>(); image.raycastTarget = false;
            image.color = new Color(.85f, .72f, .43f, 1);
        }
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
        void RefreshCursor()
        {
            if (GameInput.Instance) GameInput.Instance.UpdateMouseWorld();
            cursor = GameInput.MouseWorld;
        }
        void DrawRectangle()
        {
            var bounds = new SelectionBox(anchor.x, anchor.y, cursor.x, cursor.y);
            rectangle.anchoredPosition = new Vector2(bounds.Left, bounds.Top);
            rectangle.sizeDelta = new Vector2(bounds.Width, bounds.Height);
        }
        public Vector2Int GetGrabOffset(Vector2Int mousePosition) => Vector2Int.zero;
        public void OnDragDropMove(Vector2Int mousePosition, Vector2Int grabOffset)
        {
            cursor = mousePosition;
            if (!dragging) return;
            DrawRectangle();
        }
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
        public void OnDragDropCancel() => Cancel();
        public void Cancel()
        {
            if (DragDropManager.Instance && ReferenceEquals(DragDropManager.Instance.Target, this)) DragDropManager.Instance.ClearDragDrop();
            Active = false; dragging = false; player = null; flow = null;
            if (owner) owner.SetSelected(false);
            if (summary) summary.Hide();
            gameObject.SetActive(false);
        }
        void OnDestroy()
        {
            if (summary) summary.Hide();
            if (DragDropManager.Instance && ReferenceEquals(DragDropManager.Instance.Target, this)) DragDropManager.Instance.ClearDragDrop();
        }
        void OnApplicationFocus(bool focused)
        {
            if (!focused && Active) Cancel();
        }
    }
}
