using System;
using System.Collections.Generic;

namespace CapsuleWars.Run.Map
{
    /// <summary>
    /// Generates a seeded, branching, Slay-the-Spire-style run map one SEGMENT at a
    /// time, so the map can grow upward indefinitely. A segment is
    /// <c>rowsPerSegment</c> rows: row 0 is the entry row (Combat), the top row is a
    /// single Boss gate, and a Rest sits near the top. Edges are built by walking
    /// several bottom-to-top paths with adjacency-biased column choices; a repair pass
    /// then guarantees every node is reachable and every non-top node has an outgoing
    /// edge. Same (config, seed) → identical map (reproducible runs).
    /// </summary>
    public static class MapGenerator
    {
        // Decorrelates the RNG stream per segment so stitched segments differ.
        private const int SegmentSalt = 0x5bd1e995;

        // --- Legacy linear generator (kept so existing callers/tests compile until the
        //     run flow is migrated to the branching map with Unity up to verify). ---

        /// <summary>Legacy fixed 5-floor linear map. Superseded by <see cref="GenerateInitial"/>.</summary>
        public static RunMap GenerateDefault() => Generate(5);

        /// <summary>Legacy linear map of N floors (last is Boss). Superseded by <see cref="GenerateInitial"/>.</summary>
        public static RunMap Generate(int totalFloors)
        {
            if (totalFloors < 2) totalFloors = 2;
            var nodes = new List<MapNode>();
            for (int i = 0; i < totalFloors - 1; i++)
            {
                NodeType type;
                if (i % 4 == 1) type = NodeType.Event;
                else if (i % 4 == 2) type = NodeType.Shop;
                else if (i % 4 == 3) type = NodeType.Rest;
                else type = NodeType.Combat;
                nodes.Add(new MapNode(i, type, $"Floor {i + 1}: {type}"));
            }
            nodes.Add(new MapNode(totalFloors - 1, NodeType.Boss, $"Floor {totalFloors}: Boss"));
            return new RunMap(nodes);
        }

        /// <summary>Build the first segment (rows 0..) of a new run.</summary>
        public static RunMap GenerateInitial(MapGenConfig config, int seed)
        {
            config ??= new MapGenConfig();
            var map = new RunMap(new List<MapNode>());
            BuildSegment(map, config, seed, 0, null);
            return map;
        }

        /// <summary>
        /// Generate the next segment and stitch it on: links <paramref name="fromNode"/>
        /// (the cleared top node) into every node of the new segment's bottom row and
        /// appends the new rows above the current top.
        /// </summary>
        public static void AppendSegment(RunMap map, MapGenConfig config, int seed, int segmentIndex, MapNode fromNode)
        {
            if (map == null) return;
            config ??= new MapGenConfig();
            BuildSegment(map, config, seed, segmentIndex, fromNode);
        }

        private static void BuildSegment(RunMap map, MapGenConfig config, int seed, int segmentIndex, MapNode linkFromNode)
        {
            var rng = new System.Random(unchecked(seed ^ (segmentIndex * SegmentSalt)));
            int rows = Math.Max(2, config.rowsPerSegment);
            int baseRow = map.RowCount;     // 0 for the first segment
            int idCursor = map.Count;       // next free node id (== list position)

            // 1) Create nodes row by row (types assigned later).
            var seg = new List<List<MapNode>>(rows);
            for (int r = 0; r < rows; r++)
            {
                int count = (r == rows - 1) ? 1 : rng.Next(config.NodesMin, config.NodesMax + 1);
                var rowNodes = new List<MapNode>(count);
                for (int c = 0; c < count; c++)
                {
                    var node = new MapNode(idCursor++, baseRow + r, c, NodeType.Combat, null);
                    rowNodes.Add(node);
                    map.Nodes.Add(node);
                }
                seg.Add(rowNodes);
            }

            // 2) Walk pathCount paths from the bottom row to the top, adding edges.
            int paths = Math.Max(1, config.pathCount);
            int bottomCount = seg[0].Count;
            for (int p = 0; p < paths; p++)
            {
                var current = seg[0][p % bottomCount];   // spread starts across bottom nodes
                for (int r = 0; r < rows - 1; r++)
                {
                    var next = seg[r + 1][BiasedColumn(rng, current.Column, seg[r].Count, seg[r + 1].Count)];
                    current.AddEdge(next.Index);
                    current = next;
                }
            }

            // 3) Repair: every non-top node must have an outgoing edge.
            for (int r = 0; r < rows - 1; r++)
                foreach (var node in seg[r])
                    if (node.Edges.Count == 0)
                        node.AddEdge(seg[r + 1][BiasedColumn(rng, node.Column, seg[r].Count, seg[r + 1].Count)].Index);

            // 4) Repair: every node above the bottom must have an incoming edge (reachable).
            for (int r = 1; r < rows; r++)
                foreach (var node in seg[r])
                    if (!HasIncoming(seg[r - 1], node.Index))
                        seg[r - 1][BiasedColumn(rng, node.Column, seg[r].Count, seg[r - 1].Count)].AddEdge(node.Index);

            // 5) Stitch the previous segment's cleared top node into this bottom row.
            if (linkFromNode != null)
                foreach (var b in seg[0])
                    linkFromNode.AddEdge(b.Index);

            // 6) Node types.
            AssignTypes(seg, rows, config, rng);
        }

        private static void AssignTypes(List<List<MapNode>> seg, int rows, MapGenConfig config, System.Random rng)
        {
            foreach (var n in seg[0]) n.Type = NodeType.Combat;   // entry row
            seg[rows - 1][0].Type = NodeType.Boss;                // single boss gate

            for (int r = 1; r < rows - 1; r++)
            {
                var row = seg[r];
                for (int c = 0; c < row.Count; c++)
                {
                    var type = WeightedType(rng, config);
                    if (type == NodeType.Rest && config.noAdjacentRests && RestNearby(row, c))
                        type = NodeType.Combat;
                    row[c].Type = type;
                }
            }

            EnsureRestNearTop(seg, rows, config, rng);

            for (int r = 0; r < rows; r++)
                foreach (var n in seg[r])
                    n.DisplayLabel = $"Floor {n.Row + 1}: {n.Type}";
        }

        // Map a column in the source row to a column in the target row, jittered by ±1,
        // so paths drift toward adjacent columns rather than crossing.
        private static int BiasedColumn(System.Random rng, int fromCol, int fromCount, int toCount)
        {
            if (toCount <= 1) return 0;
            float t = fromCount <= 1 ? 0.5f : (float)fromCol / (fromCount - 1);
            int target = (int)Math.Round(t * (toCount - 1));
            int col = target + rng.Next(-1, 2);   // -1, 0, +1
            if (col < 0) col = 0;
            if (col > toCount - 1) col = toCount - 1;
            return col;
        }

        private static bool HasIncoming(List<MapNode> prevRow, int nodeId)
        {
            for (int i = 0; i < prevRow.Count; i++)
                if (prevRow[i].Edges.Contains(nodeId)) return true;
            return false;
        }

        private static NodeType WeightedType(System.Random rng, MapGenConfig config)
        {
            float total = 0f;
            foreach (var t in MapGenConfig.WeightedTypes) total += Math.Max(0f, config.WeightFor(t));
            if (total <= 0f) return NodeType.Combat;

            double roll = rng.NextDouble() * total;
            foreach (var t in MapGenConfig.WeightedTypes)
            {
                roll -= Math.Max(0f, config.WeightFor(t));
                if (roll <= 0) return t;
            }
            return NodeType.Combat;
        }

        // No two Rests adjacent within a row.
        private static bool RestNearby(List<MapNode> row, int c)
        {
            if (c - 1 >= 0 && row[c - 1].Type == NodeType.Rest) return true;
            if (c + 1 < row.Count && row[c + 1].Type == NodeType.Rest) return true;
            return false;
        }

        private static void EnsureRestNearTop(List<List<MapNode>> seg, int rows, MapGenConfig config, System.Random rng)
        {
            int band = Math.Max(0, config.restNearTopWithin);
            int lo = Math.Max(1, rows - 1 - band);   // rows just below the boss
            int hi = rows - 2;                       // last non-boss row
            if (hi < lo) return;

            for (int r = lo; r <= hi; r++)
                foreach (var n in seg[r])
                    if (n.Type == NodeType.Rest) return;   // band already has a Rest

            int rr = lo + rng.Next(hi - lo + 1);
            seg[rr][rng.Next(seg[rr].Count)].Type = NodeType.Rest;
        }
    }
}
