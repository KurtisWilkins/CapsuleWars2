using System.Collections.Generic;
using CapsuleWars.Core;
using CapsuleWars.Units.Controllers;
using UnityEngine;

namespace CapsuleWars.Abilities
{
    /// <summary>
    /// Deals flat damage to each target. M4 simplification — does not yet
    /// scale by attacker Atk, defender Def, crit, or element multipliers.
    /// Those layer in starting M5 (elements) and M6 (equipment + synergies).
    /// </summary>
    [CreateAssetMenu(fileName = "DamageEffect", menuName = "CapsuleWars/Abilities/Effects/Damage", order = 60)]
    public class DamageEffect_SO : AbilityEffectStrategy
    {
        [Tooltip("Flat damage applied to each target.")]
        [SerializeField, Min(1)] private int basePower = 25;

        [Tooltip("If true, the attacker's Atk stat is added on top of basePower.")]
        [SerializeField] private bool addAttackerAtk = false;

        public override void Apply(AbilityCastContext ctx, IReadOnlyList<IUnitRef> targets)
        {
            int extra = 0;
            if (addAttackerAtk && ctx.Source != null)
            {
                var sourceRoot = ctx.Source.GameObject != null
                    ? ctx.Source.GameObject.GetComponentInParent<UnitRoot>()
                    : null;
                if (sourceRoot != null && sourceRoot.Status != null) extra = sourceRoot.Status.Atk;
            }
            int total = basePower + extra;

            for (int i = 0; i < targets.Count; i++)
            {
                var t = targets[i];
                if (t == null || t.IsDowned) continue;
                var root = t.GameObject != null ? t.GameObject.GetComponentInParent<UnitRoot>() : null;
                if (root == null || root.Health == null) continue;
                root.Health.TakeDamage(total, ctx.Source);
            }
        }
    }
}
