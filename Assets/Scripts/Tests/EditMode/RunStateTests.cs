using System.Collections.Generic;
using CapsuleWars.Run;
using CapsuleWars.Run.Map;
using NUnit.Framework;

namespace CapsuleWars.Tests.EditMode
{
    /// <summary>
    /// Tests RunState gold, branching-graph travel (reachability, position, depth),
    /// clearing, and difficulty scaling.
    /// </summary>
    public class RunStateTests
    {
        // Tiny explicit graph: bottom row {0,1} (Combat) -> boss {2} (row 1).
        private static RunState NewState(int gold = 0)
        {
            var n0 = new MapNode(0, 0, 0, NodeType.Combat, "A");
            var n1 = new MapNode(1, 0, 1, NodeType.Combat, "B");
            var n2 = new MapNode(2, 1, 0, NodeType.Boss, "Boss");
            n0.AddEdge(2);
            n1.AddEdge(2);
            return new RunState(new RunMap(new List<MapNode> { n0, n1, n2 }), gold);
        }

        [Test]
        public void NewState_NotStarted()
        {
            var state = NewState();
            Assert.IsFalse(state.HasStarted);
            Assert.AreEqual(-1, state.CurrentNodeId);
            Assert.AreEqual(0, state.CurrentFloor);
            Assert.IsFalse(state.IsLost);
        }

        [Test]
        public void AddGold_Accumulates()
        {
            var state = NewState();
            state.AddGold(10);
            state.AddGold(15);
            Assert.AreEqual(25, state.Gold);
        }

        [Test]
        public void SpendGold_DeductsWhenAffordable_RejectsOverdraw()
        {
            var state = NewState(gold: 50);
            Assert.IsTrue(state.SpendGold(20));
            Assert.AreEqual(30, state.Gold);
            Assert.IsFalse(state.SpendGold(40));
            Assert.AreEqual(30, state.Gold);
        }

        [Test]
        public void BeforeStart_ReachableIsBottomRow()
        {
            var ids = NewState().ReachableNodeIds();
            CollectionAssert.AreEquivalent(new[] { 0, 1 }, ids);
        }

        [Test]
        public void TravelTo_BottomNode_SetsPositionAndDepth()
        {
            var state = NewState();
            Assert.IsTrue(state.TravelTo(0));
            Assert.IsTrue(state.HasStarted);
            Assert.AreEqual(0, state.CurrentNodeId);
            Assert.AreEqual(0, state.CurrentFloor);
        }

        [Test]
        public void TravelTo_UnreachableNode_Fails()
        {
            var state = NewState();
            Assert.IsFalse(state.TravelTo(2), "boss is not a bottom-row start");
            Assert.IsFalse(state.HasStarted);

            state.TravelTo(0);
            Assert.IsFalse(state.TravelTo(1), "node 1 is not connected from node 0");
        }

        [Test]
        public void AfterTravel_ReachableIsOutgoingEdges_AndCanReachBoss()
        {
            var state = NewState();
            state.TravelTo(0);
            CollectionAssert.AreEquivalent(new[] { 2 }, state.ReachableNodeIds());

            Assert.IsTrue(state.TravelTo(2));
            Assert.AreEqual(1, state.CurrentFloor);
            Assert.IsTrue(state.IsBossNode);
            Assert.IsTrue(state.IsAtTopRow);
        }

        [Test]
        public void MarkCurrentCleared_SetsVisited()
        {
            var state = NewState();
            state.TravelTo(0);
            state.MarkCurrentCleared();
            Assert.IsTrue(state.Map.Get(0).Visited);
        }

        [Test]
        public void DifficultyMultiplier_ScalesWithDepth()
        {
            var state = NewState();
            state.DifficultyPerDepth = 0.1f;
            Assert.AreEqual(1f, state.DifficultyMultiplier, 1e-4f);
            state.TravelTo(0);
            state.TravelTo(2);   // depth 1
            Assert.AreEqual(1.1f, state.DifficultyMultiplier, 1e-4f);
        }
    }
}
