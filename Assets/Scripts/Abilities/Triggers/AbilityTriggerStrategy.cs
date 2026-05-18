using UnityEngine;

namespace CapsuleWars.Abilities
{
    /// <summary>
    /// Decides when an ability should cast. Stateless — runtime state
    /// (last-cast timestamp, hit-count, etc.) lives on the
    /// <see cref="AbilityRuntime"/> passed in.
    /// Concrete subtypes: TimeBasedTrigger (cooldown), and event-based
    /// triggers (OnHit, OnKill, OnLowHp, ...) when those land.
    /// </summary>
    public abstract class AbilityTriggerStrategy : ScriptableObject
    {
        public abstract bool ShouldFire(AbilityCastContext ctx, AbilityRuntime runtime, float currentTime);
    }
}
