using System.Reflection;
using CapsuleWars.Core;
using CapsuleWars.Data.StatusEffects;
using CapsuleWars.Units.Controllers;
using NUnit.Framework;
using UnityEngine;

namespace CapsuleWars.Tests.EditMode
{
    /// <summary>
    /// BTS-B2 — the StatusEffectBehavior damage hook. Pure behavior tests + integration through
    /// UnitHealthController.TakeDamage → UnitStatusController.ModifyIncomingDamage.
    /// </summary>
    public class StatusBehaviorTests
    {
        // -------- pure behavior unit tests --------

        [Test]
        public void Marked_AddsPercentFromAllSources()
        {
            var b = ScriptableObject.CreateInstance<MarkedBehavior>();
            Assert.AreEqual(125, b.ModifyIncomingDamage(new StatusDamageContext { Kind = DamageKind.Physical }, 100));
            Assert.AreEqual(125, b.ModifyIncomingDamage(new StatusDamageContext { Kind = DamageKind.Elemental }, 100));
            Object.DestroyImmediate(b);
        }

        [Test]
        public void Frozen_AmpsPhysicalOnly()
        {
            var b = ScriptableObject.CreateInstance<FrozenBehavior>();
            Assert.AreEqual(150, b.ModifyIncomingDamage(new StatusDamageContext { Kind = DamageKind.Physical }, 100));
            Assert.AreEqual(100, b.ModifyIncomingDamage(new StatusDamageContext { Kind = DamageKind.Elemental }, 100));
            Assert.AreEqual(100, b.ModifyIncomingDamage(new StatusDamageContext { Kind = DamageKind.True }, 100));
            Object.DestroyImmediate(b);
        }

        [Test]
        public void Protected_NegatesAndConsumes()
        {
            var b = ScriptableObject.CreateInstance<ProtectedBehavior>();
            var ctx = new StatusDamageContext { Kind = DamageKind.Physical };
            Assert.AreEqual(0, b.ModifyIncomingDamage(ctx, 100));
            Assert.IsTrue(ctx.Consume);
            Object.DestroyImmediate(b);
        }

        [Test]
        public void Shield_AbsorbsFlatThenDepletes()
        {
            var b = ScriptableObject.CreateInstance<ShieldBehavior>();

            var spend = new StatusDamageContext { BehaviorValue = 30f };
            Assert.AreEqual(20, b.ModifyIncomingDamage(spend, 50));   // 50 - 30 absorbed
            Assert.AreEqual(0f, spend.BehaviorValue);
            Assert.IsTrue(spend.Consume);

            var partial = new StatusDamageContext { BehaviorValue = 30f };
            Assert.AreEqual(0, b.ModifyIncomingDamage(partial, 10));  // fully absorbed
            Assert.AreEqual(20f, partial.BehaviorValue);
            Assert.IsFalse(partial.Consume);
            Object.DestroyImmediate(b);
        }

        [Test]
        public void LastStand_ReducesOnlyBelowThreshold()
        {
            var b = ScriptableObject.CreateInstance<LastStandBehavior>();
            Assert.AreEqual(50, b.ModifyIncomingDamage(new StatusDamageContext { TargetHpFraction = 0.15f }, 100));
            Assert.AreEqual(100, b.ModifyIncomingDamage(new StatusDamageContext { TargetHpFraction = 0.5f }, 100));
            Object.DestroyImmediate(b);
        }

        // -------- integration through TakeDamage --------

        [Test]
        public void TakeDamage_AppliesMarkedBehavior()
        {
            var (go, status, health) = SpawnUnit();
            int hp0 = health.CurrentHp;
            ApplyBehavior(status, ScriptableObject.CreateInstance<MarkedBehavior>(), 0f);

            health.TakeDamage(20, null);                 // +25% → 25
            Assert.AreEqual(hp0 - 25, health.CurrentHp);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void TakeDamage_ProtectedNegatesThenIsRemoved()
        {
            var (go, status, health) = SpawnUnit();
            int hp0 = health.CurrentHp;
            ApplyBehavior(status, ScriptableObject.CreateInstance<ProtectedBehavior>(), 0f);
            Assert.AreEqual(1, status.ActiveEffects.Count);

            health.TakeDamage(50, null);                 // fully negated, status consumed
            Assert.AreEqual(hp0, health.CurrentHp);
            Assert.AreEqual(0, status.ActiveEffects.Count);

            health.TakeDamage(10, null);                 // next hit lands normally
            Assert.AreEqual(hp0 - 10, health.CurrentHp);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void TakeDamage_ShieldAbsorbsThenExpires()
        {
            var (go, status, health) = SpawnUnit();
            int hp0 = health.CurrentHp;
            ApplyBehavior(status, ScriptableObject.CreateInstance<ShieldBehavior>(), 30f);   // 30-pt shield

            health.TakeDamage(50, null);                 // 30 absorbed, 20 through
            Assert.AreEqual(hp0 - 20, health.CurrentHp);
            Assert.AreEqual(0, status.ActiveEffects.Count);   // shield spent → removed
            Object.DestroyImmediate(go);
        }

        // -------- helpers --------

        private static (GameObject, UnitStatusController, UnitHealthController) SpawnUnit()
        {
            var go = new GameObject("Unit");
            var status = go.AddComponent<UnitStatusController>();
            var health = go.AddComponent<UnitHealthController>();
            return (go, status, health);
        }

        private static void ApplyBehavior(UnitStatusController status, StatusEffectBehavior behavior, float magnitude)
        {
            var effect = ScriptableObject.CreateInstance<StatusEffect_SO>();
            SetField(effect, "behaviorSO", behavior);
            SetField(effect, "behaviorMagnitude", magnitude);
            SetField(effect, "defaultDuration", -1f);   // until cleared (no EditMode ticking anyway)
            status.ApplyStatus(effect, null);
        }

        private static void SetField(object target, string field, object value) =>
            target.GetType().GetField(field, BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(target, value);
    }
}
