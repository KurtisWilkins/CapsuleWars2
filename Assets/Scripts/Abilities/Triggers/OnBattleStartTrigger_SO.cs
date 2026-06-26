using UnityEngine;

namespace CapsuleWars.Abilities
{
    /// <summary>
    /// Fires once at battle start (Docs/05). The controller only ticks abilities during the Active phase, so this
    /// fires on the first such tick — i.e. when the runtime has never cast.
    /// </summary>
    [CreateAssetMenu(fileName = "OnBattleStartTrigger", menuName = "CapsuleWars/Abilities/Triggers/On Battle Start", order = 36)]
    public class OnBattleStartTrigger_SO : AbilityTriggerStrategy
    {
        public override bool ShouldFire(AbilityCastContext ctx, AbilityRuntime runtime, float currentTime)
            => runtime != null && runtime.LastCastTime <= float.MinValue / 2f;
    }
}
