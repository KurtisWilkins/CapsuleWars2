using CapsuleWars.Run;
using CapsuleWars.Run.Map;
using NUnit.Framework;

namespace CapsuleWars.Tests.EditMode
{
    /// <summary>
    /// Tests RunState gold management, node advancement, and completion checks.
    /// </summary>
    public class RunStateTests
    {
        private RunState NewState(int gold = 0)
        {
            var map = MapGenerator.Generate(5);
            return new RunState(map, gold);
        }

        [Test]
        public void NewState_StartsAtFloorZero()
        {
            var state = NewState();
            Assert.AreEqual(0, state.CurrentFloor);
            Assert.IsFalse(state.IsComplete);
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
        public void AddGold_IgnoresZeroOrNegative()
        {
            var state = NewState(gold: 5);
            state.AddGold(0);
            state.AddGold(-10);
            Assert.AreEqual(5, state.Gold);
        }

        [Test]
        public void SpendGold_DeductsWhenAffordable()
        {
            var state = NewState(gold: 50);
            Assert.IsTrue(state.SpendGold(20));
            Assert.AreEqual(30, state.Gold);
        }

        [Test]
        public void SpendGold_RejectsOverdraw()
        {
            var state = NewState(gold: 10);
            Assert.IsFalse(state.SpendGold(20));
            Assert.AreEqual(10, state.Gold);
        }

        [Test]
        public void AdvanceNode_MovesForward()
        {
            var state = NewState();
            state.AdvanceNode();
            Assert.AreEqual(1, state.CurrentFloor);
        }

        [Test]
        public void AdvanceNode_PastEnd_FlagsComplete()
        {
            var state = NewState();
            for (int i = 0; i < state.Map.Count; i++) state.AdvanceNode();
            Assert.IsTrue(state.IsComplete);
        }

        [Test]
        public void AdvanceNode_MarksVisited()
        {
            var state = NewState();
            var node = state.CurrentNode;
            state.AdvanceNode();
            Assert.IsTrue(node.Visited);
        }

        [Test]
        public void IsBossFloor_TrueOnLastNode()
        {
            var state = NewState();
            for (int i = 0; i < state.Map.Count - 1; i++) state.AdvanceNode();
            Assert.IsTrue(state.IsBossFloor);
        }
    }
}
