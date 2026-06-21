using System.Collections.Generic;
using CapsuleWars.Run;
using CapsuleWars.Run.Map;
using NUnit.Framework;

namespace CapsuleWars.Tests.EditMode
{
    /// <summary>
    /// Tests the branching, seeded map generator: shape, type rules, connectivity
    /// invariants, determinism, and segment stitching.
    /// </summary>
    public class MapGeneratorTests
    {
        private static MapGenConfig Config() => new MapGenConfig
        {
            rowsPerSegment = 12,
            nodesPerRowMin = 2,
            nodesPerRowMax = 4,
            pathCount = 6,
            restNearTopWithin = 2,
            noAdjacentRests = true,
        };

        // Every node reachable from some bottom-row node by following edges.
        private static HashSet<int> ReachableFromBottom(RunMap map)
        {
            var visited = new HashSet<int>();
            var queue = new Queue<MapNode>();
            foreach (var n in map.BottomRow()) { if (visited.Add(n.Index)) queue.Enqueue(n); }
            while (queue.Count > 0)
            {
                var node = queue.Dequeue();
                foreach (var next in map.Outgoing(node))
                    if (visited.Add(next.Index)) queue.Enqueue(next);
            }
            return visited;
        }

        [Test]
        public void BottomRow_IsAllCombat()
        {
            var map = MapGenerator.GenerateInitial(Config(), 12345);
            foreach (var n in map.BottomRow())
                Assert.AreEqual(NodeType.Combat, n.Type);
        }

        [Test]
        public void TopRow_IsSingleBoss()
        {
            var map = MapGenerator.GenerateInitial(Config(), 777);
            var top = map.TopRow();
            Assert.AreEqual(1, top.Count, "top row should be a single boss gate");
            Assert.AreEqual(NodeType.Boss, top[0].Type);
        }

        [Test]
        public void EveryNonTopNode_HasOutgoingEdge()
        {
            var map = MapGenerator.GenerateInitial(Config(), 42);
            int top = map.TopRowIndex;
            foreach (var n in map.Nodes)
                if (n.Row < top) Assert.Greater(n.Edges.Count, 0, $"node {n.Index} (row {n.Row}) has no outgoing edge");
        }

        [Test]
        public void EveryNode_IsReachableFromBottom()
        {
            for (int seed = 0; seed < 8; seed++)
            {
                var map = MapGenerator.GenerateInitial(Config(), seed);
                var reachable = ReachableFromBottom(map);
                Assert.AreEqual(map.Count, reachable.Count, $"seed {seed}: {map.Count - reachable.Count} unreachable node(s)");
            }
        }

        [Test]
        public void NoTwoRests_AreAdjacentInARow()
        {
            for (int seed = 0; seed < 8; seed++)
            {
                var map = MapGenerator.GenerateInitial(Config(), seed);
                for (int row = 0; row <= map.TopRowIndex; row++)
                {
                    var nodes = map.NodesInRow(row);
                    nodes.Sort((a, b) => a.Column.CompareTo(b.Column));
                    for (int i = 1; i < nodes.Count; i++)
                        Assert.IsFalse(nodes[i].Type == NodeType.Rest && nodes[i - 1].Type == NodeType.Rest,
                            $"seed {seed} row {row}: adjacent Rests");
                }
            }
        }

        [Test]
        public void NodesPerRow_WithinConfiguredRange()
        {
            var cfg = Config();
            var map = MapGenerator.GenerateInitial(cfg, 9);
            int top = map.TopRowIndex;
            for (int row = 0; row <= top; row++)
            {
                int count = map.NodesInRow(row).Count;
                if (row == top) Assert.AreEqual(1, count, "boss row");
                else Assert.IsTrue(count >= cfg.NodesMin && count <= cfg.NodesMax, $"row {row} count {count} out of range");
            }
        }

        [Test]
        public void SameSeed_ProducesIdenticalMap()
        {
            var a = MapGenerator.GenerateInitial(Config(), 2024);
            var b = MapGenerator.GenerateInitial(Config(), 2024);
            Assert.AreEqual(a.Count, b.Count);
            for (int i = 0; i < a.Count; i++)
            {
                Assert.AreEqual(a.Nodes[i].Type, b.Nodes[i].Type, $"node {i} type");
                Assert.AreEqual(a.Nodes[i].Row, b.Nodes[i].Row, $"node {i} row");
                Assert.AreEqual(a.Nodes[i].Column, b.Nodes[i].Column, $"node {i} column");
                CollectionAssert.AreEqual(a.Nodes[i].Edges, b.Nodes[i].Edges, $"node {i} edges");
            }
        }

        [Test]
        public void AppendSegment_GrowsRows_LinksTopNode_AndStaysReachable()
        {
            var cfg = Config();
            var map = MapGenerator.GenerateInitial(cfg, 555);
            int rowsBefore = map.RowCount;
            var oldBoss = map.TopRow()[0];

            MapGenerator.AppendSegment(map, cfg, 555, 1, oldBoss);

            Assert.AreEqual(rowsBefore * 2, map.RowCount, "a full new segment should be appended");
            Assert.AreEqual(NodeType.Boss, map.TopRow()[0].Type, "new top row is a boss gate");

            // The old boss now links into the new segment's bottom row.
            var newBottom = map.NodesInRow(rowsBefore);
            Assert.Greater(newBottom.Count, 0);
            foreach (var b in newBottom)
                Assert.Contains(b.Index, oldBoss.Edges, "old boss should link to each new bottom node");

            // Whole stitched map is still reachable from the original bottom row.
            Assert.AreEqual(map.Count, ReachableFromBottom(map).Count);
        }
    }
}
