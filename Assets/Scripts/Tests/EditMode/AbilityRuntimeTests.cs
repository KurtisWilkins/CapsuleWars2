using System.Collections.Generic;
using System.Reflection;
using CapsuleWars.Abilities;
using CapsuleWars.Core;
using NUnit.Framework;
using UnityEngine;

namespace CapsuleWars.Tests.EditMode
{
    /// <summary>
    /// Smoke tests for the ability runtime. Verifies that the
    /// Trigger → Targeting → Filter → Effect chain composes correctly
    /// and that lock/null guards behave as expected. Strategy-internal
    /// behavior (cooldown math, range filtering) is exercised separately
    /// via PlayMode tests later.
    /// </summary>
    public class AbilityRuntimeTests
    {
        private Ability_SO ability;
        private MockTrigger trigger;
        private MockTargeting targeting;
        private MockEffect effect;
        private MockUnit source;

        [SetUp]
        public void Setup()
        {
            ability = ScriptableObject.CreateInstance<Ability_SO>();
            trigger = ScriptableObject.CreateInstance<MockTrigger>();
            targeting = ScriptableObject.CreateInstance<MockTargeting>();
            effect = ScriptableObject.CreateInstance<MockEffect>();

            SetField(ability, "trigger", trigger);
            SetField(ability, "targeting", targeting);
            SetField(ability, "filters", new List<AbilityFilterStrategy>());
            SetField(ability, "effects", new List<AbilityEffectStrategy> { effect });

            source = new MockUnit { Id = "src" };
            targeting.Output = new List<IUnitRef> { new MockUnit { Id = "tgt" } };
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(ability);
            Object.DestroyImmediate(trigger);
            Object.DestroyImmediate(targeting);
            Object.DestroyImmediate(effect);
        }

        [Test]
        public void Tick_FiresEffect_WhenTriggerReturnsTrue()
        {
            trigger.ShouldFireResult = true;
            var rt = new AbilityRuntime(ability, source);

            bool fired = rt.Tick(1f);
            Assert.IsTrue(fired);
            Assert.AreEqual(1, effect.ApplyCallCount);
        }

        [Test]
        public void Tick_DoesNotFire_WhenTriggerReturnsFalse()
        {
            trigger.ShouldFireResult = false;
            var rt = new AbilityRuntime(ability, source);

            bool fired = rt.Tick(1f);
            Assert.IsFalse(fired);
            Assert.AreEqual(0, effect.ApplyCallCount);
        }

        [Test]
        public void Tick_RecordsLastCastTime()
        {
            trigger.ShouldFireResult = true;
            var rt = new AbilityRuntime(ability, source);

            rt.Tick(3.5f);
            Assert.AreEqual(3.5f, rt.LastCastTime, 0.0001f);
        }

        [Test]
        public void LockedRuntime_DoesNotFire()
        {
            trigger.ShouldFireResult = true;
            var rt = new AbilityRuntime(ability, source) { IsLocked = true };

            bool fired = rt.Tick(1f);
            Assert.IsFalse(fired);
            Assert.AreEqual(0, effect.ApplyCallCount);
        }

        [Test]
        public void NullTrigger_DoesNotFireAndDoesNotThrow()
        {
            SetField(ability, "trigger", null);
            var rt = new AbilityRuntime(ability, source);

            Assert.DoesNotThrow(() => rt.Tick(1f));
            Assert.AreEqual(0, effect.ApplyCallCount);
        }

        [Test]
        public void IsWeaponCompatible_TrueWhenRequirementsEmpty()
        {
            // requiredWeaponClasses is null by default
            Assert.IsTrue(ability.IsWeaponCompatible(null));
        }

        // -----------------------------------------------------------------
        // Helpers
        // -----------------------------------------------------------------

        private static void SetField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName,
                BindingFlags.NonPublic | BindingFlags.Instance);
            field?.SetValue(target, value);
        }

        private sealed class MockUnit : IUnitRef
        {
            public string Id;
            public GameObject GameObject => null;
            public Transform Transform => null;
            public Team Team => Team.Player;
            public bool IsDowned => false;
            public override string ToString() => Id;
        }

        private class MockTrigger : AbilityTriggerStrategy
        {
            public bool ShouldFireResult;
            public override bool ShouldFire(AbilityCastContext ctx, AbilityRuntime runtime, float currentTime)
                => ShouldFireResult;
        }

        private class MockTargeting : AbilityTargetingStrategy
        {
            public List<IUnitRef> Output;
            public override void Collect(AbilityCastContext ctx, List<IUnitRef> output)
            {
                if (Output != null) output.AddRange(Output);
            }
        }

        private class MockEffect : AbilityEffectStrategy
        {
            public int ApplyCallCount;
            public override void Apply(AbilityCastContext ctx, IReadOnlyList<IUnitRef> targets)
            {
                ApplyCallCount++;
            }
        }
    }
}
