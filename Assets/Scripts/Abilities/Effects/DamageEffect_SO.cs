using System.Collections.Generic;
using CapsuleWars.Core;
using CapsuleWars.Units.Controllers;
using UnityEngine;

namespace CapsuleWars.Abilities
{
    /// <summary>
    /// Deals damage to each target. M5: applies element multiplier from
    /// CombatServices.ElementChart based on attacker/defender primary
    /// elements. Crit, defender Def, and equipment scaling layer in
    /// starting M6.
    /// </summary>
    [CreateAssetMenu(fileName = "DamageEffect", menuName = "CapsuleWars/Abilities/Effects/Damage", order = 60)]
    public class DamageEffect_SO : AbilityEffectStrategy
    {
        [Tooltip("Flat damage applied to each target before element multiplier.")]
        [SerializeField, Min(1)] private int basePower = 25;

        [Tooltip("If true, the attacker's Atk stat is added on top of basePower.")]
        [SerializeField] private bool addAttackerAtk = false;

        public override void Apply(AbilityCastContext ctx, IReadOnlyList<IUnitRef> targets)
        {
            UnitRoot sourceRoot = null;
            int extra = 0;
            if (ctx.Source != null && ctx.Source.GameObject != null)
            {
                sourceRoot = ctx.Source.GameObject.GetComponentInParent<UnitRoot>();
                if (addAttackerAtk && sourceRoot != null && sourceRoot.Status != null)
                    extra = sourceRoot.Status.Atk;
            }
            int baseTotal = basePower + extra;

            var chart = CombatServices.ElementChart;
            var atkEl = sourceRoot != null && sourceRoot.Status != null ? sourceRoot.Status.PrimaryElement : null;

            for (int i = 0; i < targets.Count; i++)
            {
                var t = targets[i];
                if (t == null || t.IsDowned) continue;
                var root = t.GameObject != null ? t.GameObject.GetComponentInParent<UnitRoot>() : null;
                if (root == null || root.Health == null) continue;

                int damage = baseTotal;
                var defEl = root.Status != null ? root.Status.PrimaryElement : null;
                if (chart != null && atkEl != null && defEl != null)
                {
                    float mult = chart.GetMultiplier(atkEl.Family, defEl.Family);
                    damage = Mathf.Max(1, Mathf.RoundToInt(damage * mult));
                }

                root.Health.TakeDamage(damage, ctx.Source);
            }
        }
    }
}
