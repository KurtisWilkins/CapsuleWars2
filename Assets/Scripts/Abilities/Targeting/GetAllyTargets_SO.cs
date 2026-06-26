using System.Collections.Generic;
using CapsuleWars.Core;
using UnityEngine;

namespace CapsuleWars.Abilities
{
    /// <summary>
    /// Allied units (same team as the source), including the source itself (Docs/05). Downed allies are excluded
    /// by default; set <see cref="includeDowned"/> for revive-style abilities (the only target source for
    /// ReviveEffect / HealEffect.revivesDowned).
    /// </summary>
    [CreateAssetMenu(fileName = "GetAllyTargets", menuName = "CapsuleWars/Abilities/Targeting/Allies", order = 42)]
    public class GetAllyTargets_SO : AbilityTargetingStrategy
    {
        [Tooltip("Include downed allies (for revive abilities). Off by default — heals/buffs want living allies.")]
        [SerializeField] private bool includeDowned = false;

        public override void Collect(AbilityCastContext ctx, List<IUnitRef> output)
        {
            var registry = CombatServices.Registry;
            if (registry == null || ctx.Source == null) return;

            var units = registry.Units;
            for (int i = 0; i < units.Count; i++)
            {
                var u = units[i];
                if (u == null) continue;
                if (u.IsDowned && !includeDowned) continue;
                if (u.Team != ctx.Source.Team) continue;
                output.Add(u);   // includes self per Docs/05
            }
        }
    }
}
