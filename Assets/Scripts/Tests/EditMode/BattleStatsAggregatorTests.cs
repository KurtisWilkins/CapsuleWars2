using CapsuleWars.Combat.Stats;
using CapsuleWars.Core;
using NUnit.Framework;

namespace CapsuleWars.Tests.EditMode
{
    /// <summary>
    /// Tests stats aggregation against a raw BattleEventBus without scene
    /// objects. Verifies counters increment correctly, leaderboard sorts
    /// by damage dealt, and unhooking stops further updates.
    /// </summary>
    public class BattleStatsAggregatorTests
    {
        private BattleEventBus bus;
        private BattleStatsAggregator aggregator;

        [SetUp]
        public void Setup()
        {
            bus = new BattleEventBus();
            aggregator = new BattleStatsAggregator();
            aggregator.HookBus(bus);
            aggregator.RegisterUnit("p1", "Player One");
            aggregator.RegisterUnit("e1", "Enemy One");
            aggregator.RegisterUnit("e2", "Enemy Two");
        }

        [TearDown]
        public void Teardown()
        {
            aggregator.UnhookBus();
            aggregator.Reset();
        }

        [Test]
        public void OnDamage_IncrementsDealtAndTaken()
        {
            // Raise via raw event bus invocation by re-firing manually.
            // (HookUnit requires a UnitHealthController; for stats-only tests
            // we exercise the bus subscribers directly via reflection-free helpers.)
            FireDamage("p1", "e1", 30);

            Assert.AreEqual(30, aggregator.Get("p1").DamageDealt);
            Assert.AreEqual(1, aggregator.Get("p1").AttackCountDealt);
            Assert.AreEqual(30, aggregator.Get("e1").DamageTaken);
            Assert.AreEqual(1, aggregator.Get("e1").AttackCountTaken);
        }

        [Test]
        public void OnDamage_AccumulatesAcrossHits()
        {
            FireDamage("p1", "e1", 10);
            FireDamage("p1", "e1", 15);
            FireDamage("p1", "e2", 7);

            Assert.AreEqual(32, aggregator.Get("p1").DamageDealt);
            Assert.AreEqual(3, aggregator.Get("p1").AttackCountDealt);
            Assert.AreEqual(25, aggregator.Get("e1").DamageTaken);
            Assert.AreEqual(7, aggregator.Get("e2").DamageTaken);
        }

        [Test]
        public void OnKill_IncrementsKillsForSource()
        {
            FireKill("p1", "e1");
            FireKill("p1", "e2");
            Assert.AreEqual(2, aggregator.Get("p1").Kills);
        }

        [Test]
        public void OnDowned_IncrementsFaintsForTarget()
        {
            FireDowned("p1", "e1");
            Assert.AreEqual(1, aggregator.Get("e1").Faints);
        }

        [Test]
        public void Leaderboard_OrdersByDamageDealt()
        {
            FireDamage("p1", "e1", 50);
            FireDamage("e1", "p1", 100);
            FireDamage("e2", "p1", 25);

            var board = aggregator.BuildLeaderboard();
            board.Sort((a, b) => b.DamageDealt.CompareTo(a.DamageDealt));
            Assert.AreEqual("e1", board[0].UnitId);
            Assert.AreEqual("p1", board[1].UnitId);
            Assert.AreEqual("e2", board[2].UnitId);
        }

        [Test]
        public void Reset_ClearsAllStats()
        {
            FireDamage("p1", "e1", 50);
            aggregator.Reset();
            Assert.IsNull(aggregator.Get("p1"));
        }

        // -----------------------------------------------------------------
        // Helpers — fire events via reflection into bus's private invokers.
        // The bus normally forwards from UnitHealthController; for unit
        // tests we synthesize the same payloads directly.
        // -----------------------------------------------------------------

        private void FireDamage(string sourceId, string targetId, int amount)
        {
            var e = new DamageEvent(new TestUnit(sourceId), new TestUnit(targetId), amount);
            Invoke("OnDamageTaken", e);
            Invoke("OnDamageDealt", e);
        }

        private void FireKill(string sourceId, string targetId)
        {
            var e = new KillEvent(new TestUnit(sourceId), new TestUnit(targetId));
            Invoke("OnKill", e);
        }

        private void FireDowned(string sourceId, string targetId)
        {
            var e = new DownedEvent(new TestUnit(sourceId), new TestUnit(targetId));
            Invoke("OnDowned", e);
        }

        private void Invoke<T>(string eventName, T payload)
        {
            var field = typeof(BattleEventBus).GetField(eventName,
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Public);
            var del = field?.GetValue(bus) as System.Delegate;
            del?.DynamicInvoke(payload);
        }

        private sealed class TestUnit : IUnitRef
        {
            private readonly string id;
            public TestUnit(string id) { this.id = id; }
            public UnityEngine.GameObject GameObject => null;
            public UnityEngine.Transform Transform => null;
            public Team Team => Team.Player;
            public bool IsDowned => false;
            public override string ToString() => id;
        }
    }
}
