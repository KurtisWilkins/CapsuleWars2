using System.Collections.Generic;
using CapsuleWars.Core;
using CapsuleWars.Units.Controllers;
using UnityEngine;

namespace CapsuleWars.Abilities
{
    /// <summary>
    /// Bring a downed ally back at <see cref="revivePercent"/> of MaxHp (Docs/05). Living targets are skipped.
    /// (HealEffect_SO has a revivesDowned shortcut; this is the standalone effect the doc names.)
    /// </summary>
    [CreateAssetMenu(fileName = "ReviveEffect", menuName = "CapsuleWars/Abilities/Effects/Revive", order = 62)]
    public class ReviveEffect_SO : AbilityEffectStrategy
    {
        [Tooltip("Fraction of MaxHp the revived unit returns at.")]
        [SerializeField, Range(0.01f, 1f)] private float revivePercent = 0.5f;

        public override void Apply(AbilityCastContext ctx, IReadOnlyList<IUnitRef> targets)
        {
            for (int i = 0; i < targets.Count; i++)
            {
                var t = targets[i];
                var root = t != null && t.GameObject != null ? t.GameObject.GetComponentInParent<UnitRoot>() : null;
                if (root == null || root.Health == null) continue;
                if (!root.Health.IsDowned) continue;
                root.Health.RestoreToPercent(revivePercent);
            }
        }
    }
}
