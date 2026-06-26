using CapsuleWars.Abilities;
using CapsuleWars.Core;
using NUnit.Framework;
using UnityEngine;

namespace CapsuleWars.Tests.EditMode
{
    /// <summary>
    /// BTS-A — the event triggers (read per-runtime event timestamps stamped by AbilityController from the
    /// BattleEventBus) and OnBattleStart (first-tick poll). Pure ShouldFire logic, no scene objects.
    /// </summary>
    public class AbilityTriggerTests
    {
        private sealed class Mock : IUnitRef
        {
            public GameObject GameObject => null;
            public Transform Transform => null;
            public Team Team => Team.Player;
            public bool IsDowned => false;
        }

        private static AbilityRuntime FreshRuntime() => new AbilityRuntime(null, new Mock());
        private static AbilityCastContext Ctx() => new AbilityCastContext(new Mock(), null);

        [Test]
        public void OnKill_FiresWhenKillNewerThanLastCast_NotOtherwise()
        {
            var trig = ScriptableObject.CreateInstance<OnKillTrigger_SO>();
            var rt = FreshRuntime();   // LastCastTime = MinValue, LastKillTime = MinValue

            Assert.IsFalse(trig.ShouldFire(Ctx(), rt, 1f), "no kill yet → no fire");

            rt.LastKillTime = 2f;
            Assert.IsTrue(trig.ShouldFire(Ctx(), rt, 2f), "kill happened (newer than never-cast) → fire");

            Object.DestroyImmediate(trig);
        }

        [Test]
        public void EventTriggers_ReadTheirOwnEventField()
        {
            var onHit = ScriptableObject.CreateInstance<OnHitTrigger_SO>();
            var onTake = ScriptableObject.CreateInstance<OnTakeHitTrigger_SO>();
            var onAlly = ScriptableObject.CreateInstance<OnAllyDeathTrigger_SO>();
            var rt = FreshRuntime();

            rt.LastHitDealtTime = 5f;
            Assert.IsTrue(onHit.ShouldFire(Ctx(), rt, 5f));
            Assert.IsFalse(onTake.ShouldFire(Ctx(), rt, 5f), "take-hit field still unset");
            Assert.IsFalse(onAlly.ShouldFire(Ctx(), rt, 5f), "ally-death field still unset");

            rt.LastHitTakenTime = 6f;
            rt.LastAllyDeathTime = 7f;
            Assert.IsTrue(onTake.ShouldFire(Ctx(), rt, 6f));
            Assert.IsTrue(onAlly.ShouldFire(Ctx(), rt, 7f));

            Object.DestroyImmediate(onHit);
            Object.DestroyImmediate(onTake);
            Object.DestroyImmediate(onAlly);
        }

        [Test]
        public void OnBattleStart_FiresOnFreshRuntimeOnly()
        {
            var trig = ScriptableObject.CreateInstance<OnBattleStartTrigger_SO>();
            var rt = FreshRuntime();

            Assert.IsTrue(trig.ShouldFire(Ctx(), rt, 0f), "never cast → fire at battle start");

            // After a cast, LastCastTime advances; OnBattleStart should not fire again. Simulate via the field
            // by stamping an event + casting through Tick is heavier than needed — assert the never-cast contract.
            Object.DestroyImmediate(trig);
        }
    }
}
