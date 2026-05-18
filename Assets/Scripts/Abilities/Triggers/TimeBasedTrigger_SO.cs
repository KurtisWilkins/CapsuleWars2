using UnityEngine;

namespace CapsuleWars.Abilities
{
    /// <summary>
    /// Fires every <see cref="cooldown"/> seconds. Also supports a one-time
    /// initial delay before the first cast.
    /// </summary>
    [CreateAssetMenu(fileName = "TimeBasedTrigger", menuName = "CapsuleWars/Abilities/Triggers/Time-Based", order = 30)]
    public class TimeBasedTrigger_SO : AbilityTriggerStrategy
    {
        [Tooltip("Seconds between casts.")]
        [SerializeField, Min(0.1f)] private float cooldown = 5f;

        [Tooltip("Extra delay applied to the first cast on top of the cooldown. Useful so abilities don't all fire on frame 1.")]
        [SerializeField, Min(0f)] private float initialDelay = 0f;

        public override bool ShouldFire(AbilityCastContext ctx, AbilityRuntime runtime, float currentTime)
        {
            if (runtime.LastCastTime <= float.MinValue / 2f)
            {
                // First cast: respect the initial delay, measured from time=0.
                return currentTime >= initialDelay;
            }
            return currentTime - runtime.LastCastTime >= cooldown;
        }
    }
}
