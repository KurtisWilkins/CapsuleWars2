using System.Collections.Generic;
using CapsuleWars.Run;
using CapsuleWars.Run.Map;
using UnityEngine;
using UnityEngine.UI;

namespace CapsuleWars.UI.Map
{
    /// <summary>
    /// Renders the run's branching map as clickable nodes + connecting edge lines inside
    /// a vertical ScrollRect, data-driven off <see cref="RunState.Map"/>. Highlights the
    /// current node, the reachable (clickable) nodes, visited nodes, and locked nodes;
    /// clicking a reachable node travels there via <see cref="RunController.TravelToNode"/>.
    /// Rebuilds whenever the run state refreshes and auto-scrolls to the current position.
    ///
    /// Place on the map panel. Content layout: nodes are positioned by (column → x,
    /// row → y) in <see cref="content"/>'s local space (y grows upward from the bottom),
    /// so <see cref="content"/> should be a tall child of a masked ScrollRect viewport
    /// with its pivot/anchor at bottom-centre.
    /// </summary>
    public class MapView : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private RectTransform content;
        [SerializeField] private MapNodeView nodePrefab;
        [SerializeField] private Image edgePrefab;   // thin line image, pivot (0.5,0.5)

        [Header("Layout")]
        [SerializeField] private float rowSpacing = 170f;
        [SerializeField] private float colSpacing = 150f;
        [SerializeField] private float bottomMargin = 120f;
        [SerializeField] private float edgeThickness = 6f;
        [SerializeField] private Color edgeColor = new Color(1f, 1f, 1f, 0.35f);

        private RunController controller;
        private readonly List<GameObject> spawned = new();

        private void Awake() => EnsureForeground();

        private void OnEnable()
        {
            controller = FindAnyObjectByType<RunController>();
            if (controller != null) controller.OnStateRefreshed += Rebuild;
            Rebuild();
        }

        private void OnDisable()
        {
            if (controller != null) controller.OnStateRefreshed -= Rebuild;
        }

        public void Rebuild()
        {
            Clear();
            var state = RunSession.Current;
            if (state == null || state.Map == null || content == null || nodePrefab == null) return;

            var map = state.Map;
            var reachable = new HashSet<int>(state.ReachableNodeIds());
            int rows = map.RowCount;

            content.sizeDelta = new Vector2(content.sizeDelta.x, rows * rowSpacing + bottomMargin * 2f);

            // Node positions (column centred per row; y grows up from the bottom margin).
            var pos = new Dictionary<int, Vector2>();
            for (int row = 0; row < rows; row++)
            {
                var rowNodes = map.NodesInRow(row);
                int n = rowNodes.Count;
                for (int i = 0; i < n; i++)
                {
                    var node = rowNodes[i];
                    float x = (node.Column - (n - 1) * 0.5f) * colSpacing;
                    float y = bottomMargin + row * rowSpacing;
                    pos[node.Index] = new Vector2(x, y);
                }
            }

            // Edges first (drawn behind nodes).
            if (edgePrefab != null)
                foreach (var node in map.Nodes)
                {
                    if (!pos.TryGetValue(node.Index, out var a)) continue;
                    foreach (var e in node.Edges)
                        if (pos.TryGetValue(e, out var b)) SpawnEdge(a, b);
                }

            // Nodes.
            foreach (var node in map.Nodes)
            {
                if (!pos.TryGetValue(node.Index, out var p)) continue;
                var view = Instantiate(nodePrefab, content);
                ((RectTransform)view.transform).anchoredPosition = p;
                view.Bind(node, StateFor(state, node, reachable), OnNodeClicked);
                spawned.Add(view.gameObject);
            }

            ScrollToCurrent(state, pos);
        }

        private static MapNodeVisualState StateFor(RunState s, MapNode node, HashSet<int> reachable)
        {
            if (node.Index == s.CurrentNodeId) return MapNodeVisualState.Current;
            if (reachable.Contains(node.Index)) return MapNodeVisualState.Reachable;
            if (node.Visited) return MapNodeVisualState.Visited;
            return MapNodeVisualState.Locked;
        }

        private void OnNodeClicked(int id)
        {
            if (controller != null) controller.TravelToNode(id);
        }

        private void SpawnEdge(Vector2 a, Vector2 b)
        {
            var edge = Instantiate(edgePrefab, content);
            var rt = edge.rectTransform;
            Vector2 d = b - a;
            rt.anchoredPosition = (a + b) * 0.5f;
            rt.sizeDelta = new Vector2(d.magnitude, edgeThickness);
            rt.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg);
            edge.color = edgeColor;
            edge.raycastTarget = false;
            edge.transform.SetAsFirstSibling();   // keep edges behind node buttons
            spawned.Add(edge.gameObject);
        }

        private void ScrollToCurrent(RunState s, Dictionary<int, Vector2> pos)
        {
            if (scrollRect == null || content == null) return;
            float h = content.sizeDelta.y;
            float y = (s.HasStarted && pos.TryGetValue(s.CurrentNodeId, out var p)) ? p.y : 0f;
            scrollRect.verticalNormalizedPosition = h > 1f ? Mathf.Clamp01(y / h) : 0f;
        }

        private void Clear()
        {
            for (int i = 0; i < spawned.Count; i++) if (spawned[i] != null) Destroy(spawned[i]);
            spawned.Clear();
        }

        // Keep the map panel rendering + raycasting above other map UI.
        private void EnsureForeground()
        {
            var canvas = GetComponent<Canvas>();
            if (canvas == null) canvas = gameObject.AddComponent<Canvas>();
            canvas.overrideSorting = true;
            canvas.sortingOrder = 50;
            if (GetComponent<GraphicRaycaster>() == null) gameObject.AddComponent<GraphicRaycaster>();
        }
    }
}
