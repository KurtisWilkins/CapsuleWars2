using System.Collections.Generic;
using CapsuleWars.Core;
using CapsuleWars.Data.StatusEffects;
using CapsuleWars.Units.Controllers;
using UnityEngine;

namespace CapsuleWars.Abilities
{
    /// <summary>
    /// Applies a status effect to each target. The effect's own
    /// configuration determines duration, stacking, and behavior —
    /// this strategy just routes the application.
    /// </summary>
    [CreateAssetMenu(fileName = "ApplyStatusEffect", menuName = "CapsuleWars/Abilities/Effects/Apply Status", order = 62)]
    public class ApplyStatusEffect_SO : AbilityEffectStrategy
    {
        [Tooltip("Status effect to apply to each target.")]
        [SerializeField] private StatusEffect_SO statusEffect;

        public override void Apply(AbilityCastContext ctx, IReadOnlyList<IUnitRef> targets)
        {
            if (statusEffect == null) return;

            for (int i = 0; i < targets.Count; i++)
            {
                var t = targets[i];
                if (t == null || t.IsDowned) continue;
                var root = t.GameObject != null ? t.GameObject.GetComponentInParent<UnitRoot>() : null;
                if (root == null || root.Status == null) continue;
                root.Status.ApplyStatus(statusEffect, ctx.Source);
            }
        }
    }
}
