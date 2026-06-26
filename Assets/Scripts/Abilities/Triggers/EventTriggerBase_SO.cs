using UnityEngine;

namespace CapsuleWars.Abilities
{
    /// <summary>
    /// Base for combat-event triggers (Docs/05): fire when the unit's event — stamped on the runtime by
    /// <c>AbilityController</c> from the <c>BattleEventBus</c> — is newer than this ability's last cast. Supports
    /// an optional inner cooldown for spam control. Pure (reads only the runtime + time) → EditMode-testable.
    /// </summary>
    public abstract class EventTriggerBase_SO : AbilityTriggerStrategy
    {
        [Tooltip("Minimum seconds between casts from this trigger (spam control). 0 = none.")]
        [SerializeField, Min(0f)] protected float innerCooldown = 0f;

        /// <summary>The timestamp of the relevant event for this runtime (MinValue = it never happened).</summary>
        protected abstract float EventTime(AbilityRuntime runtime);

        public override bool ShouldFire(AbilityCastContext ctx, AbilityRuntime runtime, float currentTime)
        {
            if (runtime == null) return false;
            float ev = EventTime(runtime);
            if (ev <= float.MinValue / 2f) return false;                 // event never happened
            if (ev <= runtime.LastCastTime) return false;                // already cast since the event
            if (runtime.LastCastTime > float.MinValue / 2f && currentTime - runtime.LastCastTime < innerCooldown)
                return false;                                            // inner cooldown not elapsed
            return true;
        }
    }
}
