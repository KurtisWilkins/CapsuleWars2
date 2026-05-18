using System.Collections.Generic;
using System.Reflection;
using CapsuleWars.Core;
using CapsuleWars.Data.StatusEffects;
using CapsuleWars.Units.Controllers;
using NUnit.Framework;
using UnityEngine;

namespace CapsuleWars.Tests.EditMode
{
    /// <summary>
    /// Tests status effect application, stat modification, ticking,
    /// and expiry. Uses real GameObjects with UnitStatus + UnitHealth
    /// components so the stat fold-in is exercised end-to-end.
    /// </summary>
    public class StatusEffectTests
    {
        private GameObject go;
        private UnitStatusController status;
        private UnitHealthController health;

        [SetUp]
        public void Setup()
        {
            go = new GameObject("TestUnit");
            status = go.AddComponent<UnitStatusController>();
            health = go.AddComponent<UnitHealthController>();
        }

        [TearDown]
        public void Teardown()
        {
            if (go != null) Object.DestroyImmediate(go);
        }

        [Test]
        public void ApplyStatus_AddsToActiveList()
        {
            var effect = BuildStatusEffect(duration: 5f);
            status.ApplyStatus(effect, null);
            Assert.AreEqual(1, status.ActiveEffects.Count);
        }

        [Test]
        public void Refresh_DoesNotStack()
        {
            var effect = BuildStatusEffect(duration: 5f, stack: StackBehavior.Refresh);
            status.ApplyStatus(effect, null);
            status.ApplyStatus(effect, null);
            status.ApplyStatus(effect, null);
            Assert.AreEqual(1, status.ActiveEffects.Count);
        }

        [Test]
        public void Independent_AllowsMultipleInstances()
        {
            var effect = BuildStatusEffect(duration: 5f, stack: StackBehavior.Independent);
            status.ApplyStatus(effect, null);
            status.ApplyStatus(effect, null);
            Assert.AreEqual(2, status.ActiveEffects.Count);
        }

        [Test]
        public void StatBuff_PercentAtk_AppliesAndReverts()
        {
            int before = status.Atk;
            var effect = BuildStatusEffect(
                duration: 5f,
                buffs: new[] { new StatBuff { stat = StatType.Atk, modType = StatBuffModType.Percent, amount = 50 } });
            status.ApplyStatus(effect, null);

            // +50% of baseAtk (20) = +10; result 30
            Assert.AreEqual(before + 10, status.Atk);

            status.ClearAllStatuses();
            Assert.AreEqual(before, status.Atk);
        }

        [Test]
        public void StatBuff_FlatDef_StacksAcrossEffects()
        {
            int before = status.Def;
            var e1 = BuildStatusEffect(duration: 5f,
                buffs: new[] { new StatBuff { stat = StatType.Def, modType = StatBuffModType.Flat, amount = 5 } });
            var e2 = BuildStatusEffect(duration: 5f,
                buffs: new[] { new StatBuff { stat = StatType.Def, modType = StatBuffModType.Flat, amount = 3 } });
            status.ApplyStatus(e1, null);
            status.ApplyStatus(e2, null);

            Assert.AreEqual(before + 8, status.Def);
        }

        [Test]
        public void PreventsAction_ExposesCannotActFlag()
        {
            var effect = BuildStatusEffect(duration: 5f, preventsAction: true);
            status.ApplyStatus(effect, null);
            Assert.IsTrue(status.CannotAct);
        }

        [Test]
        public void CannotUseAbilities_TrueWhenSilencedOrCannotAct()
        {
            var silenced = BuildStatusEffect(duration: 5f, preventsAbilities: true);
            status.ApplyStatus(silenced, null);
            Assert.IsTrue(status.CannotUseAbilities);

            status.ClearAllStatuses();
            var stunned = BuildStatusEffect(duration: 5f, preventsAction: true);
            status.ApplyStatus(stunned, null);
            Assert.IsTrue(status.CannotUseAbilities); // CannotAct implies CannotUseAbilities
        }

        [Test]
        public void ClearStatus_RemovesEffect()
        {
            var effect = BuildStatusEffect(duration: 5f);
            status.ApplyStatus(effect, null);
            status.ClearStatus(effect);
            Assert.AreEqual(0, status.ActiveEffects.Count);
        }

        // -----------------------------------------------------------------
        // Helpers
        // -----------------------------------------------------------------

        private static StatusEffect_SO BuildStatusEffect(
            float duration = 5f,
            StackBehavior stack = StackBehavior.Refresh,
            StatusEffectKind kind = StatusEffectKind.Buff,
            IEnumerable<StatBuff> buffs = null,
            int tickAmount = 0,
            bool preventsAction = false,
            bool preventsMovement = false,
            bool preventsAbilities = false)
        {
            var effect = ScriptableObject.CreateInstance<StatusEffect_SO>();
            SetField(effect, "defaultDuration", duration);
            SetField(effect, "stackBehavior", stack);
            SetField(effect, "kind", kind);
            SetField(effect, "maxStacks", 99);
            if (buffs != null) SetField(effect, "statBuffs", new List<StatBuff>(buffs));
            SetField(effect, "tickAmount", tickAmount);
            SetField(effect, "preventsAction", preventsAction);
            SetField(effect, "preventsMovement", preventsMovement);
            SetField(effect, "preventsAbilities", preventsAbilities);
            return effect;
        }

        private static void SetField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName,
                BindingFlags.NonPublic | BindingFlags.Instance);
            field?.SetValue(target, value);
        }
    }
}
