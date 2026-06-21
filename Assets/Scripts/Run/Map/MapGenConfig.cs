using System;
using UnityEngine;

namespace CapsuleWars.Run.Map
{
    /// <summary>
    /// Designer-tunable settings for branching map generation. Plain serializable
    /// class (no asset needed) so it can be set on a MonoBehaviour and used by the
    /// generator in EditMode tests. Boss is rule-placed (segment gate) and is not
    /// part of the weighted pool; Combat fills the bottom row by rule.
    /// </summary>
    [Serializable]
    public class MapGenConfig
    {
        [Header("Shape")]
        [Tooltip("Rows in one segment (bottom row .. boss row).")]
        [Min(2)] public int rowsPerSegment = 13;
        [Tooltip("Fewest nodes generated in a row.")]
        [Min(1)] public int nodesPerRowMin = 2;
        [Tooltip("Most nodes generated in a row.")]
        [Min(1)] public int nodesPerRowMax = 4;
        [Tooltip("Number of bottom-to-top paths walked to create the edge graph.")]
        [Min(1)] public int pathCount = 6;

        [Header("Type weights (Boss is rule-placed; bottom row is always Combat)")]
        [Min(0f)] public float combatWeight = 4f;
        [Min(0f)] public float eliteWeight = 1f;
        [Min(0f)] public float shopWeight = 1f;
        [Min(0f)] public float eventWeight = 1.5f;
        [Min(0f)] public float restWeight = 1f;
        [Min(0f)] public float treasureWeight = 1f;

        [Header("Rules")]
        [Tooltip("Place a Rest somewhere within this many rows below the boss row.")]
        [Min(0)] public int restNearTopWithin = 2;
        [Tooltip("Disallow two Rest nodes in the same row adjacent to each other / a Rest directly above a Rest.")]
        public bool noAdjacentRests = true;

        /// <summary>Weighted pool used for ordinary nodes (everything except Boss).</summary>
        public static readonly NodeType[] WeightedTypes =
        {
            NodeType.Combat, NodeType.Elite, NodeType.Shop,
            NodeType.Event, NodeType.Rest, NodeType.Treasure
        };

        public float WeightFor(NodeType t)
        {
            switch (t)
            {
                case NodeType.Combat: return combatWeight;
                case NodeType.Elite: return eliteWeight;
                case NodeType.Shop: return shopWeight;
                case NodeType.Event: return eventWeight;
                case NodeType.Rest: return restWeight;
                case NodeType.Treasure: return treasureWeight;
                default: return 0f;
            }
        }

        /// <summary>Validated min/max (max ≥ min ≥ 1).</summary>
        public int NodesMin => Mathf.Max(1, Mathf.Min(nodesPerRowMin, nodesPerRowMax));
        public int NodesMax => Mathf.Max(NodesMin, nodesPerRowMax);
    }
}
