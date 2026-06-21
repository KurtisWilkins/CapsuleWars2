using System;
using CapsuleWars.Run;
using CapsuleWars.Run.Map;
using UnityEngine;
using UnityEngine.UI;

namespace CapsuleWars.UI.Map
{
    /// <summary>Visual state of a node, driving its colour/interactability.</summary>
    public enum MapNodeVisualState { Locked, Reachable, Current, Visited }

    /// <summary>
    /// One clickable map node. Sits on the node prefab (Button + Image + optional Text).
    /// <see cref="Bind"/> colours/labels it by <see cref="NodeType"/> and visual state, and
    /// wires the click to travel only when the node is currently reachable.
    /// </summary>
    public class MapNodeView : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Image icon;
        [SerializeField] private Text label;

        public int NodeId { get; private set; }

        public void Bind(MapNode node, MapNodeVisualState state, Action<int> onClick)
        {
            NodeId = node.Index;

            if (label != null) label.text = ShortLabel(node.Type);
            if (icon != null) icon.color = Tint(TypeColor(node.Type), state);

            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.interactable = state == MapNodeVisualState.Reachable;
                if (state == MapNodeVisualState.Reachable)
                {
                    int id = node.Index;
                    button.onClick.AddListener(() => onClick?.Invoke(id));
                }
            }
        }

        private static string ShortLabel(NodeType t)
        {
            switch (t)
            {
                case NodeType.Combat: return "C";
                case NodeType.Elite: return "E";
                case NodeType.Shop: return "$";
                case NodeType.Event: return "?";
                case NodeType.Rest: return "R";
                case NodeType.Treasure: return "T";
                case NodeType.Boss: return "BOSS";
                default: return "?";
            }
        }

        private static Color TypeColor(NodeType t)
        {
            switch (t)
            {
                case NodeType.Combat: return new Color(0.80f, 0.32f, 0.32f);
                case NodeType.Elite: return new Color(0.90f, 0.22f, 0.50f);
                case NodeType.Shop: return new Color(0.92f, 0.80f, 0.30f);
                case NodeType.Event: return new Color(0.40f, 0.62f, 0.92f);
                case NodeType.Rest: return new Color(0.40f, 0.85f, 0.50f);
                case NodeType.Treasure: return new Color(0.85f, 0.68f, 0.30f);
                case NodeType.Boss: return new Color(0.70f, 0.25f, 0.85f);
                default: return Color.gray;
            }
        }

        private static Color Tint(Color baseColor, MapNodeVisualState state)
        {
            switch (state)
            {
                case MapNodeVisualState.Current: return Color.Lerp(baseColor, Color.white, 0.55f);
                case MapNodeVisualState.Reachable: return baseColor;
                case MapNodeVisualState.Visited: return Color.Lerp(baseColor, Color.black, 0.45f);
                default: // Locked — dim + translucent
                    var c = Color.Lerp(baseColor, Color.black, 0.55f);
                    c.a = 0.45f;
                    return c;
            }
        }
    }
}
