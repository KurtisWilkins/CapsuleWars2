using CapsuleWars.Units.Controllers;
using UnityEngine;

namespace CapsuleWars.Abilities
{
    /// <summary>
    /// Fires once when the unit's HP% first drops to/below <see cref="hpThreshold"/> (Docs/05). Latched via the
    /// runtime's last-cast (fires a single time; re-arming on recovery is a later refinement).
    /// </summary>
    [CreateAssetMenu(fileName = "OnLowHpTrigger", menuName = "CapsuleWars/Abilities/Triggers/On Low HP", order = 35)]
    public class OnLowHpTrigger_SO : AbilityTriggerStrategy
    {
        [Tooltip("HP fraction at/below which the ability fires (e.g. 0.3 = 30%).")]
        [SerializeField, Range(0.01f, 1f)] private float hpThreshold = 0.3f;

        public override bool ShouldFire(AbilityCastContext ctx, AbilityRuntime runtime, float currentTime)
        {
            if (runtime == null || runtime.LastCastTime > float.MinValue / 2f) return false;   // fire once
            if (ctx.Source == null || ctx.Source.GameObject == null) return false;
            var root = ctx.Source.GameObject.GetComponentInParent<UnitRoot>();
            if (root == null || root.Health == null || root.Status == null) return false;
            float pct = (float)root.Health.CurrentHp / Mathf.Max(1, root.Status.MaxHp);
            return pct <= hpThreshold;
        }
    }
}
