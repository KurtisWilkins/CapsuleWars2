using System.Collections.Generic;
using CapsuleWars.Core;
using CapsuleWars.Units.Controllers;
using UnityEngine;

namespace CapsuleWars.Abilities
{
    /// <summary>
    /// Restores HP to each target. Heals do not exceed MaxHp. Does not
    /// affect downed units by default — revives are a separate effect
    /// (see ReviveEffect_SO, scheduled with the priest class in M5+).
    /// </summary>
    [CreateAssetMenu(fileName = "HealEffect", menuName = "CapsuleWars/Abilities/Effects/Heal", order = 61)]
    public class HealEffect_SO : AbilityEffectStrategy
    {
        [Tooltip("Flat HP restored to each target.")]
        [SerializeField, Min(1)] private int basePower = 20;

        [Tooltip("If true, downed units are revived to basePower HP. If false, downed units are skipped.")]
        [SerializeField] private bool revivesDowned = false;

        public override void Apply(AbilityCastContext ctx, IReadOnlyList<IUnitRef> targets)
        {
            for (int i = 0; i < targets.Count; i++)
            {
                var t = targets[i];
                if (t == null) continue;
                var root = t.GameObject != null ? t.GameObject.GetComponentInParent<UnitRoot>() : null;
                if (root == null || root.Health == null || root.Status == null) continue;

                if (root.Health.IsDowned)
                {
                    if (!revivesDowned) continue;
                    float pct = Mathf.Clamp01((float)basePower / Mathf.Max(1, root.Status.MaxHp));
                    root.Health.RestoreToPercent(pct);
                    continue;
                }

                int currentMissing = root.Status.MaxHp - root.Health.CurrentHp;
                if (currentMissing <= 0) continue;
                int healed = Mathf.Min(basePower, currentMissing);
                int newPct = root.Health.CurrentHp + healed;
                float ratio = (float)newPct / Mathf.Max(1, root.Status.MaxHp);
                root.Health.RestoreToPercent(ratio);
            }
        }
    }
}
